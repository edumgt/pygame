import random
import sys
from enum import Enum, auto

import pygame

from config import ASSETS, BALANCE, DISPLAY, GRAY, RED, ROAD_EDGE, WHITE, YELLOW
from systems.resource_loader import ResourceLoader


class SceneState(Enum):
    START = auto()
    PLAYING = auto()
    PAUSED = auto()
    GAME_OVER = auto()


class CarRaceGame:
    def __init__(self):
        pygame.init()
        pygame.mixer.init()

        self.screen = pygame.display.set_mode((DISPLAY.width, DISPLAY.height))
        pygame.display.set_caption(DISPLAY.title)
        self.clock = pygame.time.Clock()
        self.font = pygame.font.SysFont(None, 36)

        self.loader = ResourceLoader()
        self.car_img = self.loader.load_image(ASSETS.car_image, (50, 90), (30, 180, 255))
        self.obs_img = self.loader.load_image(ASSETS.obstacle_image, (50, 80), (220, 100, 20))
        self.crash_sound = self.loader.load_sound(ASSETS.crash_sfx, volume=0.8)
        self.has_bgm = self.loader.load_bgm(ASSETS.bgm, volume=0.5)

        self.state = SceneState.START
        self.running = True
        self.reset_round()

    def reset_round(self):
        self.car_x = DISPLAY.width // 2 - 25
        self.car_y = DISPLAY.height - 110
        self.car_speed = BALANCE.car_speed

        self.obs_x = random.randint(80, DISPLAY.width - 130)
        self.obs_y = -80
        self.obs_speed = BALANCE.initial_obstacle_speed

        self.score = 0
        self.line_speed = BALANCE.initial_line_speed
        self.lines = [pygame.Rect(DISPLAY.width // 2 - 5, i, 10, 50) for i in range(0, DISPLAY.height, 90)]

    def start_bgm(self):
        if self.has_bgm:
            pygame.mixer.music.play(-1)

    def stop_bgm(self):
        if self.has_bgm:
            pygame.mixer.music.stop()

    def handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
                return

            if event.type == pygame.KEYDOWN:
                self.handle_keydown(event.key)

    def handle_keydown(self, key: int):
        if self.state == SceneState.START:
            if key == pygame.K_SPACE:
                self.state = SceneState.PLAYING
                self.start_bgm()
            elif key == pygame.K_ESCAPE:
                self.running = False

        elif self.state == SceneState.PLAYING:
            if key == pygame.K_p:
                self.state = SceneState.PAUSED
            elif key == pygame.K_ESCAPE:
                self.running = False

        elif self.state == SceneState.PAUSED:
            if key == pygame.K_p:
                self.state = SceneState.PLAYING
            elif key == pygame.K_ESCAPE:
                self.running = False

        elif self.state == SceneState.GAME_OVER:
            if key == pygame.K_y:
                self.reset_round()
                self.state = SceneState.PLAYING
                self.start_bgm()
            elif key in (pygame.K_n, pygame.K_ESCAPE):
                self.running = False

    def update_playing(self):
        keys = pygame.key.get_pressed()
        if keys[pygame.K_LEFT] and self.car_x > 50:
            self.car_x -= self.car_speed
        if keys[pygame.K_RIGHT] and self.car_x < DISPLAY.width - 80:
            self.car_x += self.car_speed

        self.obs_y += self.obs_speed
        if self.obs_y > DISPLAY.height:
            self.obs_y = -80
            self.obs_x = random.randint(80, DISPLAY.width - 130)
            self.score += 1
            self.obs_speed += BALANCE.obstacle_speed_increase
            self.line_speed += BALANCE.line_speed_increase

        car_rect = pygame.Rect(self.car_x, self.car_y, 50, 90)
        obs_rect = pygame.Rect(self.obs_x, self.obs_y, 50, 80)
        if car_rect.colliderect(obs_rect):
            if self.crash_sound:
                self.crash_sound.play()
            self.stop_bgm()
            self.state = SceneState.GAME_OVER

        self.move_lines()

    def move_lines(self):
        for line in self.lines:
            line.y += self.line_speed
            if line.y > DISPLAY.height:
                line.y = -90

    def draw_base_scene(self):
        self.screen.fill(GRAY)
        pygame.draw.rect(self.screen, ROAD_EDGE, [0, 0, 50, DISPLAY.height])
        pygame.draw.rect(self.screen, ROAD_EDGE, [DISPLAY.width - 50, 0, 50, DISPLAY.height])
        for line in self.lines:
            pygame.draw.rect(self.screen, YELLOW, line)

    def draw_score(self):
        text = self.font.render(f"Score: {self.score}", True, WHITE)
        self.screen.blit(text, (10, 10))

    def draw_overlay_text(self, message: str, sub_message: str):
        text1 = self.font.render(message, True, RED if "Game Over" in message else WHITE)
        text2 = self.font.render(sub_message, True, WHITE)
        self.screen.blit(text1, (DISPLAY.width // 2 - text1.get_width() // 2, DISPLAY.height // 2 - 50))
        self.screen.blit(text2, (DISPLAY.width // 2 - text2.get_width() // 2, DISPLAY.height // 2))

    def render(self):
        self.draw_base_scene()
        self.screen.blit(self.car_img, (self.car_x, self.car_y))
        self.screen.blit(self.obs_img, (self.obs_x, self.obs_y))
        self.draw_score()

        if self.state == SceneState.START:
            self.draw_overlay_text("Press SPACE to Start", "ESC: Quit")
        elif self.state == SceneState.PAUSED:
            self.draw_overlay_text("Paused", "Press P to Resume")
        elif self.state == SceneState.GAME_OVER:
            self.draw_overlay_text("Game Over!", "Play again? (Y/N)")

        pygame.display.flip()

    def run(self):
        while self.running:
            self.handle_events()

            if self.state == SceneState.PLAYING:
                self.update_playing()

            self.render()
            self.clock.tick(DISPLAY.fps)

        pygame.quit()
        sys.exit()
