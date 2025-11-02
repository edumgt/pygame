import pygame
import random
import sys
import os

# ==============================
# 🚗 Pygame 초기 설정
# ==============================
pygame.init()
pygame.mixer.init()

WIDTH, HEIGHT = 400, 600
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("🚗 자동차 경주 (Play Again 기능 포함)")

WHITE = (255, 255, 255)
GRAY = (60, 60, 60)
RED = (200, 30, 30)
YELLOW = (255, 230, 50)

clock = pygame.time.Clock()
FPS = 60

# ==============================
# 🎨 파일 경로
# ==============================
CAR_IMG_PATH = "car.png"
OBS_IMG_PATH = "obstacle.png"
BGM_PATH = "bgm.mp3"
CRASH_PATH = "crash.wav"

# ==============================
# 🖼 이미지 로드
# ==============================
car_img = pygame.image.load(CAR_IMG_PATH).convert_alpha()
car_img = pygame.transform.smoothscale(car_img, (50, 90))

obs_img = pygame.image.load(OBS_IMG_PATH).convert_alpha()
obs_img = pygame.transform.smoothscale(obs_img, (50, 80))

# ==============================
# 🎵 사운드 로드
# ==============================
if os.path.exists(BGM_PATH):
    pygame.mixer.music.load(BGM_PATH)
    pygame.mixer.music.set_volume(0.5)
    pygame.mixer.music.play(-1)
else:
    print("⚠️ 배경음악 없음")

crash_sound = None
if os.path.exists(CRASH_PATH):
    crash_sound = pygame.mixer.Sound(CRASH_PATH)
    crash_sound.set_volume(0.8)

# ==============================
# 🧩 변수 초기화 함수
# ==============================
def reset_game():
    global car_x, car_y, car_speed, obs_x, obs_y, obs_speed, score, line_speed, lines
    car_x = WIDTH // 2 - 25
    car_y = HEIGHT - 110
    car_speed = 5
    obs_x = random.randint(80, WIDTH - 130)
    obs_y = -80
    obs_speed = 5
    score = 0
    line_speed = 5
    lines = [pygame.Rect(WIDTH // 2 - 5, i, 10, 50) for i in range(0, HEIGHT, 90)]

reset_game()
font = pygame.font.SysFont(None, 36)

# ==============================
# 🔧 함수 정의
# ==============================
def draw_lines():
    for line in lines:
        pygame.draw.rect(screen, YELLOW, line)

def move_lines():
    for line in lines:
        line.y += line_speed
        if line.y > HEIGHT:
            line.y = -90

def show_score():
    text = font.render(f"Score: {score}", True, WHITE)
    screen.blit(text, (10, 10))

def game_over():
    if crash_sound:
        crash_sound.play()
    pygame.mixer.music.stop()

    text1 = font.render("💥 Game Over!", True, RED)
    text2 = font.render("Play again? (Y/N)", True, WHITE)

    screen.blit(text1, (WIDTH // 2 - 100, HEIGHT // 2 - 40))
    screen.blit(text2, (WIDTH // 2 - 120, HEIGHT // 2 + 10))
    pygame.display.flip()

    waiting = True
    while waiting:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_y:
                    reset_game()
                    if os.path.exists(BGM_PATH):
                        pygame.mixer.music.play(-1)
                    waiting = False
                elif event.key == pygame.K_n:
                    pygame.quit()
                    sys.exit()

# ==============================
# 🕹 메인 루프
# ==============================
while True:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            sys.exit()

    keys = pygame.key.get_pressed()
    if keys[pygame.K_LEFT] and car_x > 50:
        car_x -= car_speed
    if keys[pygame.K_RIGHT] and car_x < WIDTH - 80:
        car_x += car_speed

    obs_y += obs_speed
    if obs_y > HEIGHT:
        obs_y = -80
        obs_x = random.randint(80, WIDTH - 130)
        score += 1
        obs_speed += 0.2
        line_speed += 0.05

    # 충돌 감지
    car_rect = pygame.Rect(car_x, car_y, 50, 90)
    obs_rect = pygame.Rect(obs_x, obs_y, 50, 80)
    if car_rect.colliderect(obs_rect):
        game_over()

    # 화면 업데이트
    move_lines()
    screen.fill(GRAY)
    pygame.draw.rect(screen, (40, 40, 40), [0, 0, 50, HEIGHT])
    pygame.draw.rect(screen, (40, 40, 40), [WIDTH - 50, 0, 50, HEIGHT])
    draw_lines()
    screen.blit(car_img, (car_x, car_y))
    screen.blit(obs_img, (obs_x, obs_y))
    show_score()

    pygame.display.flip()
    clock.tick(FPS)
