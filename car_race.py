import pygame
import random
import sys
import io
from PIL import Image
import xml.etree.ElementTree as ET

# ==== SVG → PNG 변환용 (Pillow 내장 변환) ====
# SVG의 색상/도형만 간단히 파싱해서 Pillow로 그리는 임시 대체 렌더러
def svg_to_surface(svg_path, width, height, color=(100, 150, 255)):
    img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    # 단순 색 사각형만 채우는 대체 렌더 (실제 SVG 렌더링 없이 플레이용)
    from PIL import ImageDraw
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 0, width, height], fill=color)
    return pygame.image.fromstring(img.tobytes(), img.size, img.mode).convert_alpha()

# ==== Pygame 초기화 ====
pygame.init()
pygame.mixer.init()

WIDTH, HEIGHT = 400, 600
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("🚗 자동차 경주 (cairo DLL 없이)")

WHITE = (255, 255, 255)
GRAY = (60, 60, 60)
RED = (200, 30, 30)
YELLOW = (255, 230, 50)
clock = pygame.time.Clock()
FPS = 60

# ==== car.svg / obstacle.svg 파일 경로 ====
car_svg_path = "car.svg"
obstacle_svg_path = "obstacle_cone.svg"

# ==== 이미지 로드 ====
car_img = svg_to_surface(car_svg_path, 50, 90, color=(0, 120, 255))
obs_img = svg_to_surface(obstacle_svg_path, 50, 80, color=(255, 100, 0))

# ==== 변수 ====
car_x = WIDTH // 2 - 25
car_y = HEIGHT - 110
car_speed = 5
obs_x = random.randint(80, WIDTH - 130)
obs_y = -80
obs_speed = 5
score = 0
font = pygame.font.SysFont(None, 36)
line_speed = 5
lines = [pygame.Rect(WIDTH // 2 - 5, i, 10, 50) for i in range(0, HEIGHT, 90)]

# ==== 함수 ====
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
    text = font.render("💥 Game Over!", True, RED)
    screen.blit(text, (WIDTH // 2 - 100, HEIGHT // 2 - 20))
    pygame.display.flip()
    pygame.time.wait(2000)
    pygame.quit()
    sys.exit()

# ==== 메인 루프 ====
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

    car_rect = pygame.Rect(car_x, car_y, 50, 90)
    obs_rect = pygame.Rect(obs_x, obs_y, 50, 80)
    if car_rect.colliderect(obs_rect):
        game_over()

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
