import cv2
import socket
import json
import time
import math
import mediapipe as mp
from mediapipe.framework.formats import landmark_pb2

# ========== FLAG DE DEBUG ==========
DRAW_LANDMARKS = True  # ← define como False para desativar visualização

# ========== CONFIGURAÇÃO DO UDP ==========
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# ========== IMPORTAÇÕES ==========
BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
HandLandmarkerResult = mp.tasks.vision.HandLandmarkerResult
VisionRunningMode = mp.tasks.vision.RunningMode
ImageFormat = mp.ImageFormat
MPImage = mp.Image

drawing_utils = mp.solutions.drawing_utils
drawing_styles = mp.solutions.drawing_styles

# ========== VARIÁVEIS GLOBAIS ==========
latest_result = None
last_gesture = None
frame_counter = 0
last_report_time = time.time()
total_payload_size = 0
last_directions = {}
last_on_screen_states = {}

# ========== FUNÇÕES AUXILIARES ==========
def distance(a, b):
    return math.sqrt((a.x - b.x) ** 2 + (a.y - b.y) ** 2 + (a.z - b.z) ** 2)
   

# Recebe os pontos da mão, calcula a distância entre os pontos da mão
def classify_gesture(landmarks):
    """Classifica gesto com base nos landmarks da mão (lista de 21 pontos)."""
    thumb_tip = landmarks[4]
    wrist = landmarks[0]
    index_tip = landmarks[8]
    middle_tip = landmarks[12]
    ring_tip = landmarks[16]
    pinky_tip = landmarks[20]

    # pinch_dist = distance(thumb_tip, index_tip)
    # thumb_extension = distance(thumb_tip, wrist)
    # folded_fingers = [distance(f, wrist) for f in [index_tip, middle_tip, ring_tip, pinky_tip]]
    # print(f"[DEBUG] pinch={pinch_dist:.3f}, thumb_ext={thumb_extension:.3f}, folded={folded_fingers}")q
    # print(f"[DEBUG] pinch={pinch_dist:.3f}")

    # Heurística para PINCH
    pinch_dist = distance(thumb_tip, index_tip)
    if pinch_dist < 0.05:
        return "pinch"

    # Heurística para THUMBS_UP
    thumb_extended = distance(thumb_tip, wrist) > 0.45
    other_fingers_folded = all(
        distance(finger_tip, wrist) < 0.35
        for finger_tip in [index_tip, middle_tip, ring_tip, pinky_tip]
    )
    if thumb_extended and other_fingers_folded:
        return "thumbs_up"

    return None

def draw_hand_landmarks(frame, result):
    """Desenha landmarks no frame com base no resultado do HandLandmarker."""
    if not result.hand_landmarks:
        return

    for landmarks in result.hand_landmarks:
        landmark_list = landmark_pb2.NormalizedLandmarkList(
            landmark=[
                landmark_pb2.NormalizedLandmark(x=lm.x, y=lm.y, z=lm.z)
                for lm in landmarks
            ]
        )

        drawing_utils.draw_landmarks(
            frame,
            landmark_list,
            mp.solutions.hands.HAND_CONNECTIONS,
            drawing_styles.get_default_hand_landmarks_style(),
            drawing_styles.get_default_hand_connections_style()
        )


def is_hand_on_screen(landmarks):
    """Verifica se todos os landmarks estão dentro dos limites [0, 1]."""
    for lm in landmarks:
        if not (0.0 <= lm.x <= 1.0 and 0.0 <= lm.y <= 1.0):
            return False
    return True

def is_index_finger_pointing_up_or_down(landmarks):
    # Dedo indicador
    mcp = landmarks[5]   # base do dedo indicador
    pip = landmarks[6]   # meio do dedo
    dip = landmarks[7]   # quase ponta
    tip = landmarks[8]   # ponta

    # Outros dedos (médio, anelar, mindinho)
    other_fingers_tips = [landmarks[12], landmarks[16], landmarks[20]]
    wrist = landmarks[0]

    # Distância do indicador ao pulso (em Y)
    dist_index = abs(tip.y - wrist.y)

    # Verifica se os outros dedos estão pelo menos 50% mais próximos do pulso
    others_folded = all(
        abs(finger_tip.y - wrist.y) < 0.5 * dist_index
        for finger_tip in other_fingers_tips
    )

    is_up = tip.y < pip.y < mcp.y
    is_down = tip.y > pip.y > mcp.y

    if is_up and others_folded:
        return "up"
    elif is_down and others_folded:
        return "down"
    else:
        return None


# ========== CALLBACK ==========
def send_result(result: HandLandmarkerResult, output_image: mp.Image, timestamp_ms: int):
    global latest_result, last_gesture, frame_counter, last_report_time, total_payload_size

    if not result.hand_landmarks:
        return

    gestures = []
    hands_data = []

    for i, hand_landmarks in enumerate(result.hand_landmarks):
        gesture = classify_gesture(hand_landmarks)
        landmarks = [{"x": lm.x, "y": lm.y, "z": lm.z} for lm in hand_landmarks]
        pinch_distance = distance(hand_landmarks[4], hand_landmarks[8])
        handed = result.handedness[i][0].category_name  # "Left" ou "Right"

        # Verifica se está no ecrã
        on_screen = is_hand_on_screen(hand_landmarks)
         # Verifica se está a apontar
        if gesture is None:
            
            if handed.lower() == "right":
                direction = is_index_finger_pointing_up_or_down(hand_landmarks)
                # If a direction is found, assign it as the gesture
                if direction is not None:
                    gesture = direction
        
        # Debug
        if gesture and (handed not in last_directions or last_directions[handed] != gesture):
             print(f"[GESTO] {gesture.upper()} detetado na mão {handed.upper()}")
             last_directions[handed] = gesture
             
        gestures.append(gesture)
        hands_data.append({
            "landmarks": landmarks,
            "gesture": gesture,
            "pinch_distance": pinch_distance,
            "handed": handed,
            "on_screen": on_screen
        })

    # Envia toda a lista de mãos
    message_dict = {"hands": hands_data}
    message_json = json.dumps(message_dict)
    payload_bytes = message_json.encode()
    total_payload_size += len(payload_bytes)
    sock.sendto(payload_bytes, (UDP_IP, UDP_PORT))

    # Mostra na consola se o gesto mudou (apenas com mão visível e gesto válido)
    for i, gesture in enumerate(gestures):
        handed = result.handedness[i][0].category_name
        on_screen = hands_data[i]["on_screen"]

        if not on_screen or gesture is None:
            continue  # ignora se a mão não estiver visível ou sem gesto

        if handed not in last_directions:
            last_directions[handed] = None

        if gesture != last_directions[handed]:
            if gesture == "pinch":
                print(f"[GESTO] {gesture.upper()} detetado na mão {handed.upper()}")
            last_directions[handed] = gesture

    frame_counter += 1
    now = time.time()
    if now - last_report_time >= 5.0:
        elapsed = now - last_report_time
        fps = frame_counter / elapsed
        avg_payload = total_payload_size / frame_counter if frame_counter > 0 else 0
        #print(f"[PERFORMANCE] {fps:.2f} deteções/s | Payload médio: {avg_payload:.1f} bytes")
        last_report_time = now
        frame_counter = 0
        total_payload_size = 0

    latest_result = result


# ========== CONFIGURAÇÃO DO LANDMARKER ==========
MODEL_PATH = "D:/Users/joaop/Documents/UNI/IAV/MediaPipe/Python-MediaPipe/hand_landmarker.task"

options = HandLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=MODEL_PATH),
    running_mode=VisionRunningMode.LIVE_STREAM,
    num_hands=2, # mudança para duas mãos
    result_callback=send_result
)

# ========== LOOP DE VÍDEO ==========
with HandLandmarker.create_from_options(options) as landmarker:
    cap = cv2.VideoCapture(0)
    timestamp = 0

    print("Pressione 'q' para sair...")

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            print("Erro ao capturar imagem da webcam.")
            break

        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = MPImage(image_format=ImageFormat.SRGB, data=rgb_frame)

        landmarker.detect_async(mp_image, timestamp)
        timestamp += 1

        if DRAW_LANDMARKS and latest_result:
            draw_hand_landmarks(frame, latest_result)

        cv2.imshow("Hand Tracker", frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()
    sock.close()
