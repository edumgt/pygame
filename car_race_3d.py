from ursina import *
from ursina.shaders import lit_with_shadows_shader
from panda3d.core import NodePath
import os, math

app = Ursina(title='🚗 3D Car Orbit Viewer (Stable)', borderless=False)
window.size = (1200, 800)
window.color = color.rgb(15, 15, 20)

# --------------------------
# 모델 경로
# --------------------------
MODEL_PATH = os.path.join('models', 'car.glb')
if not os.path.exists(MODEL_PATH):
    print(f'❌ 모델 파일이 없습니다: {MODEL_PATH}')
    exit()

# --------------------------
# 모델 로드 및 스케일 자동 조정
# --------------------------
car = Entity(model=MODEL_PATH, shader=lit_with_shadows_shader)

try:
    np = NodePath(car.model)
    min_p, max_p = np.get_tight_bounds()
    if min_p and max_p:
        size_x = max_p.x - min_p.x
        size_y = max_p.y - min_p.y
        size_z = max_p.z - min_p.z
        max_dim = max(size_x, size_y, size_z)
        if max_dim <= 0 or max_dim != max_dim:
            max_dim = 1.0
        car.scale = 4 / max_dim
        # ✅ NodePath.set_y 충돌 방지를 위해 position 직접 설정
        car.position = Vec3(0, -min_p.y * car.scale * 0.5, 0)
        print(f"✅ 모델 크기 자동 조정: {car.scale:.3f}")
    else:
        raise ValueError("get_tight_bounds() 결과 없음")
except Exception as e:
    print(f"⚠️ bounds 계산 실패: {e}")
    car.scale = 1

car.rotation_y = 180
car.color = color.white
car.shininess = 64

# --------------------------
# 바닥 + 조명
# --------------------------
plane = Entity(model='plane', scale=40, color=color.rgb(70, 70, 70),
               position=(0, 0, 0), shader=lit_with_shadows_shader)

sun = DirectionalLight(y=10, z=-10, x=6, shadows=True, color=color.rgb(255, 250, 230))
sun.look_at(Vec3(0, 0, 0))
AmbientLight(color=color.rgba(255, 255, 255, 220))
side = DirectionalLight(x=-8, y=5, z=5, color=color.rgb(200, 220, 255))
side.look_at(Vec3(0, 0, 0))

# --------------------------
# Orbit 카메라
# --------------------------
orbit_distance = 8.0
orbit_yaw = 30
orbit_pitch = 20
orbit_sensitivity = 200
zoom_speed = 1.0

camera.fov = 70
camera.clip_plane_far = 300

def update_camera():
    yaw_r = math.radians(orbit_yaw)
    pitch_r = math.radians(orbit_pitch)
    cam_x = orbit_distance * math.sin(yaw_r) * math.cos(pitch_r)
    cam_y = orbit_distance * math.sin(pitch_r)
    cam_z = orbit_distance * math.cos(yaw_r) * math.cos(pitch_r)
    camera.position = (cam_x, cam_y + 1.5, cam_z)
    camera.look_at(car.position + Vec3(0, 1.0, 0))

def input(key):
    global orbit_distance
    if key == 'scroll up':
        orbit_distance = max(2, orbit_distance - zoom_speed)
    elif key == 'scroll down':
        orbit_distance = min(40, orbit_distance + zoom_speed)

mouse_prev = None

def update():
    global orbit_yaw, orbit_pitch, mouse_prev
    if held_keys['left mouse']:
        if mouse_prev is None:
            mouse_prev = Vec2(mouse.x, mouse.y)  # ✅ Vec3 → Vec2 수정
        delta = Vec2(mouse.x, mouse.y) - mouse_prev
        orbit_yaw -= delta.x * orbit_sensitivity
        orbit_pitch += delta.y * orbit_sensitivity
        orbit_pitch = clamp(orbit_pitch, -20, 60)
        mouse_prev = Vec2(mouse.x, mouse.y)
    else:
        mouse_prev = None
    update_camera()

# --------------------------
# 안내 텍스트
# --------------------------
Text(
    "왼쪽 드래그: 360° 회전 | 휠: 줌 | ESC: 종료",
    origin=(0, 0), y=-.45, color=color.white, scale=1.05
)

update_camera()
app.run()
