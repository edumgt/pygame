from ursina import *
from panda3d.core import loadPrcFileData, NodePath, Loader, Filename, DirectionalLight, AmbientLight
import os, math

# ----------------------------
# 렌더링 설정
# ----------------------------
loadPrcFileData('', 'framebuffer-srgb true')
loadPrcFileData('', 'gl-version 3 3')
loadPrcFileData('', 'textures-power-2 up')
loadPrcFileData('', 'framebuffer-multisample 1')
loadPrcFileData('', 'multisamples 4')

app = Ursina(title='🚗 PBR Car Viewer (Exposure Fixed)', borderless=False)
window.size = (1280, 800)
window.color = color.rgb(15, 15, 18)

MODEL_PATH = os.path.join('models', 'car.glb')
if not os.path.exists(MODEL_PATH):
    print(f'❌ 모델 파일이 없습니다: {MODEL_PATH}')
    exit()

# ----------------------------
# 모델 로드 (원본 텍스처 유지)
# ----------------------------
loader = Loader.get_global_ptr()
car_np = loader.load_sync(Filename.from_os_specific(MODEL_PATH))
car_np = NodePath(car_np)
car_np.reparent_to(render)
render.set_shader_auto()

# 스케일 자동 조정
try:
    min_p, max_p = car_np.get_tight_bounds()
    max_dim = max(max_p.x - min_p.x, max_p.y - min_p.y, max_p.z - min_p.z)
    scale_factor = 4 / max_dim if max_dim > 0 else 1
    car_np.set_scale(scale_factor)
    car_np.set_pos(0, -min_p.y * scale_factor * 0.5, 0)
    print(f"✅ 모델 스케일 조정 완료 (scale={scale_factor:.3f})")
except Exception as e:
    print(f"⚠️ 모델 bounds 계산 실패: {e}")

# ----------------------------
# 조명 (안정형)
# ----------------------------
# 햇빛
sun_light = DirectionalLight('sun')
sun_light.set_color((0.8, 0.8, 0.7, 1))  # 노란빛 약하게
sun_np = render.attach_new_node(sun_light)
sun_np.set_hpr(45, -60, 0)
render.set_light(sun_np)

# 보조광 (하늘 반사)
fill_light = DirectionalLight('fill')
fill_light.set_color((0.5, 0.55, 0.7, 1))
fill_np = render.attach_new_node(fill_light)
fill_np.set_hpr(-60, 40, 0)
render.set_light(fill_np)

# 환경광 (전체 밝기)
ambient = AmbientLight('ambient')
ambient.set_color((0.35, 0.35, 0.4, 1))
ambient_np = render.attach_new_node(ambient)
render.set_light(ambient_np)

# ----------------------------
# 바닥면
# ----------------------------
plane = Entity(model='plane', scale=40, position=(0, 0, 0), color=color.rgb(50, 50, 50))
plane.specular = color.rgb(90, 90, 90)

# ----------------------------
# Orbit Camera
# ----------------------------
orbit_distance = 8.0
orbit_yaw = 30
orbit_pitch = 15
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
    camera.look_at(car_np.get_pos() + Vec3(0, 1, 0))

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
            mouse_prev = Vec2(mouse.x, mouse.y)
        delta = Vec2(mouse.x, mouse.y) - mouse_prev
        orbit_yaw -= delta.x * orbit_sensitivity
        orbit_pitch += delta.y * orbit_sensitivity
        orbit_pitch = clamp(orbit_pitch, -15, 60)
        mouse_prev = Vec2(mouse.x, mouse.y)
    else:
        mouse_prev = None
    update_camera()

# ----------------------------
# 텍스트 안내
# ----------------------------
Text(
    "왼쪽 드래그: 회전 | 휠: 줌 | ESC: 종료 | 노출 보정 / 텍스처 유지형",
    origin=(0, 0), y=-.45, color=color.white, scale=1.05
)

update_camera()
app.run()
