import random
import sys
from dataclasses import dataclass

import pygame

from config import ASSETS, BALANCE, DISPLAY, GRAY, ITEMS, RED, SAVE, WHITE, YELLOW
from systems.resource_loader import ResourceLoader
from systems.save_system import SaveSystem


class SceneState:
    START = "start"
    PLAYING = "playing"
    PAUSED = "paused"
    GAME_OVER = "game_over"


@dataclass(frozen=True)
class VehicleStat:
    name: str
    acceleration: float
    max_speed: float
    handling: float
    color: tuple[int, int, int]


@dataclass(frozen=True)
class TrackTheme:
    name: str
    bg_color: tuple[int, int, int]
    road_color: tuple[int, int, int]
    edge_color: tuple[int, int, int]
    line_color: tuple[int, int, int]


class CarRaceGame:
    def __init__(self):
        pygame.init()
        pygame.mixer.init()

        self.screen = pygame.display.set_mode((DISPLAY.width, DISPLAY.height))
        pygame.display.set_caption(DISPLAY.title)
        self.clock = pygame.time.Clock()
        self.font = pygame.font.SysFont(None, 28)
        self.large_font = pygame.font.SysFont(None, 34)

        self.loader = ResourceLoader()
        self.save_system = SaveSystem(SAVE.highscore_file)
        self.high_score = self.save_system.load_highscore()

        self.car_img = self.loader.load_image(ASSETS.car_image, (50, 90), (30, 180, 255))
        self.obs_img = self.loader.load_image(ASSETS.obstacle_image, (50, 80), (220, 100, 20))
        self.crash_sound = self.loader.load_sound(ASSETS.crash_sfx, volume=0.8)
        self.has_bgm = self.loader.load_bgm(ASSETS.bgm, volume=0.5)

        self.vehicles = [
            VehicleStat("Sprint", acceleration=0.36, max_speed=8.0, handling=0.16, color=(80, 200, 255)),
            VehicleStat("Interceptor", acceleration=0.28, max_speed=9.8, handling=0.12, color=(255, 180, 60)),
            VehicleStat("Drift", acceleration=0.24, max_speed=8.8, handling=0.2, color=(220, 90, 255)),
        ]
        self.selected_vehicle_idx = 0

        self.track_themes = [
            TrackTheme("도심", bg_color=(54, 64, 82), road_color=GRAY, edge_color=(35, 35, 35), line_color=YELLOW),
            TrackTheme("사막", bg_color=(183, 145, 87), road_color=(105, 88, 75), edge_color=(130, 98, 58), line_color=(250, 230, 120)),
            TrackTheme("야간 네온", bg_color=(16, 12, 34), road_color=(35, 35, 60), edge_color=(95, 35, 130), line_color=(40, 255, 220)),
        ]
        self.current_theme = self.track_themes[0]

        self.state = SceneState.START
        self.running = True
        self.reset_round()

    def lane_positions(self):
        left_bound = 60
        right_bound = DISPLAY.width - 110
        if BALANCE.lane_count == 1:
            return [DISPLAY.width // 2 - 25]
        interval = (right_bound - left_bound) / (BALANCE.lane_count - 1)
        return [int(left_bound + interval * idx) for idx in range(BALANCE.lane_count)]

    def reset_round(self):
        self.current_theme = random.choice(self.track_themes)
        self.frame_counter = 0
        self.lane_x = self.lane_positions()
        self.player_target_x = self.lane_x[len(self.lane_x) // 2]
        self.car_x = float(self.player_target_x)
        self.car_velocity_x = 0.0
        self.car_y = DISPLAY.height - 110

        self.enemy_speed = BALANCE.initial_enemy_speed
        self.enemies = []
        self.spawn_initial_enemies()

        self.score = 0
        self.line_speed = BALANCE.initial_line_speed
        self.lines = [pygame.Rect(DISPLAY.width // 2 - 5, i, 10, 50) for i in range(0, DISPLAY.height, 90)]

        self.shield_active = False
        self.shield_item = None
        self.next_shield_spawn = ITEMS.shield_spawn_interval_frames

        self.boost_timer = 0
        self.boost_cooldown = 0
        self.boost_used_in_round = False

        self.mission = self.generate_mission()
        self.touch_start = None
        self.mobile_left_pressed = False
        self.mobile_right_pressed = False
        self.mobile_boost_pressed = False

    def spawn_initial_enemies(self):
        for idx in range(BALANCE.enemy_count):
            lane = idx % len(self.lane_x)
            self.enemies.append(
                {
                    "lane": lane,
                    "target_lane": lane,
                    "x": self.lane_x[lane],
                    "y": -220 * idx - 80,
                    "speed_mul": random.uniform(0.92, 1.15),
                    "ai_timer": random.randint(25, 110),
                }
            )

    def generate_mission(self):
        mission_type = random.choice(["survive", "no_boost"])
        if mission_type == "survive":
            return {
                "type": "survive",
                "label": "미션: 20초 생존",
                "target_frames": DISPLAY.fps * 20,
                "completed": False,
                "failed": False,
            }
        return {
            "type": "no_boost",
            "label": "미션: 25초 노부스트",
            "target_frames": DISPLAY.fps * 25,
            "completed": False,
            "failed": False,
        }

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
            elif event.type == pygame.FINGERDOWN:
                self.touch_start = (event.x * DISPLAY.width, event.y * DISPLAY.height)
            elif event.type == pygame.FINGERUP:
                self.handle_swipe((event.x * DISPLAY.width, event.y * DISPLAY.height))
                self.touch_start = None
            elif event.type == pygame.MOUSEBUTTONDOWN:
                self.handle_mobile_button_press(event.pos, True)
                self.touch_start = event.pos
            elif event.type == pygame.MOUSEBUTTONUP:
                self.handle_mobile_button_press(event.pos, False)
                if self.touch_start:
                    self.handle_swipe(event.pos)
                self.touch_start = None

    def handle_keydown(self, key: int):
        if self.state == SceneState.START:
            if key == pygame.K_SPACE:
                self.state = SceneState.PLAYING
                self.start_bgm()
            elif key == pygame.K_1:
                self.selected_vehicle_idx = 0
            elif key == pygame.K_2:
                self.selected_vehicle_idx = 1
            elif key == pygame.K_3:
                self.selected_vehicle_idx = 2
            elif key == pygame.K_ESCAPE:
                self.running = False

        elif self.state == SceneState.PLAYING:
            if key == pygame.K_p:
                self.state = SceneState.PAUSED
            elif key == pygame.K_LSHIFT:
                self.activate_boost()
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

    def activate_boost(self):
        if self.boost_cooldown == 0:
            self.boost_timer = BALANCE.boost_duration_frames
            self.boost_cooldown = BALANCE.boost_cooldown_frames
            self.boost_used_in_round = True
            if self.mission["type"] == "no_boost" and not self.mission["completed"]:
                self.mission["failed"] = True

    def handle_mobile_button_press(self, pos, is_pressed):
        left_btn, right_btn, boost_btn = self.mobile_button_rects()
        if left_btn.collidepoint(pos):
            self.mobile_left_pressed = is_pressed
        if right_btn.collidepoint(pos):
            self.mobile_right_pressed = is_pressed
        if boost_btn.collidepoint(pos):
            self.mobile_boost_pressed = is_pressed
            if is_pressed and self.state == SceneState.PLAYING:
                self.activate_boost()

    def handle_swipe(self, end_pos):
        if not self.touch_start or self.state != SceneState.PLAYING:
            return
        dx = end_pos[0] - self.touch_start[0]
        dy = abs(end_pos[1] - self.touch_start[1])
        if abs(dx) < 35 or abs(dx) < dy:
            return
        if dx > 0:
            self.player_target_x = min(self.player_target_x + 90, self.lane_x[-1])
        else:
            self.player_target_x = max(self.player_target_x - 90, self.lane_x[0])

    def update_playing(self):
        self.frame_counter += 1
        keys = pygame.key.get_pressed()
        selected = self.vehicles[self.selected_vehicle_idx]

        left_pressed = keys[pygame.K_LEFT] or self.mobile_left_pressed
        right_pressed = keys[pygame.K_RIGHT] or self.mobile_right_pressed

        accel = selected.acceleration
        max_speed = min(selected.max_speed, BALANCE.max_player_speed)
        if self.boost_timer > 0:
            max_speed += BALANCE.boost_extra_speed

        if left_pressed:
            self.car_velocity_x -= accel
        elif right_pressed:
            self.car_velocity_x += accel
        else:
            self.car_velocity_x *= 0.85

        self.car_velocity_x = max(-max_speed, min(max_speed, self.car_velocity_x))
        self.car_x += self.car_velocity_x

        snap_factor = BALANCE.lane_change_speed + selected.handling
        self.car_x += (self.player_target_x - self.car_x) * snap_factor
        self.car_x = max(50, min(DISPLAY.width - 100, self.car_x))

        if self.boost_timer > 0:
            self.boost_timer -= 1
        if self.boost_cooldown > 0:
            self.boost_cooldown -= 1

        self.update_enemies()
        self.update_shield_item()
        self.update_mission_state()
        self.handle_collisions()
        self.move_lines()

        if self.frame_counter % BALANCE.score_tick_frames == 0:
            self.score += 1

    def update_enemies(self):
        player_lane = min(range(len(self.lane_x)), key=lambda idx: abs(self.lane_x[idx] - self.car_x))

        for enemy in self.enemies:
            enemy["ai_timer"] -= 1
            if enemy["ai_timer"] <= 0:
                enemy["ai_timer"] = random.randint(30, 100)
                close_to_player = abs(enemy["y"] - self.car_y) < 180
                if close_to_player and random.random() < 0.65:
                    enemy["target_lane"] = player_lane  # 블로킹
                else:
                    delta = random.choice([-1, 0, 1])
                    enemy["target_lane"] = max(0, min(len(self.lane_x) - 1, enemy["lane"] + delta))

            enemy["x"] += (self.lane_x[enemy["target_lane"]] - enemy["x"]) * 0.13
            if abs(enemy["x"] - self.lane_x[enemy["target_lane"]]) < 3:
                enemy["lane"] = enemy["target_lane"]

            enemy["y"] += self.enemy_speed * enemy["speed_mul"]

            if enemy["y"] > DISPLAY.height + 100:
                enemy["y"] = random.randint(-360, -80)
                enemy["lane"] = random.randint(0, len(self.lane_x) - 1)
                enemy["target_lane"] = enemy["lane"]
                enemy["x"] = self.lane_x[enemy["lane"]]
                self.score += 2
                self.enemy_speed += BALANCE.enemy_speed_increase
                self.line_speed += BALANCE.line_speed_increase

    def update_shield_item(self):
        if self.shield_item:
            self.shield_item.y += ITEMS.shield_fall_speed
            if self.shield_item.y > DISPLAY.height:
                self.shield_item = None

        if not self.shield_active and not self.shield_item:
            self.next_shield_spawn -= 1
            if self.next_shield_spawn <= 0:
                x = random.randint(60, DISPLAY.width - 90)
                w, h = ITEMS.shield_size
                self.shield_item = pygame.Rect(x, -h, w, h)
                self.next_shield_spawn = ITEMS.shield_spawn_interval_frames

    def update_mission_state(self):
        if self.mission["completed"] or self.mission["failed"]:
            return

        if self.frame_counter >= self.mission["target_frames"]:
            self.mission["completed"] = True
            self.score += 30

    def handle_collisions(self):
        car_rect = pygame.Rect(self.car_x, self.car_y, 50, 90)

        if self.shield_item and car_rect.colliderect(self.shield_item):
            self.shield_active = True
            self.shield_item = None

        for enemy in self.enemies:
            enemy_rect = pygame.Rect(enemy["x"], enemy["y"], 50, 80)
            if car_rect.colliderect(enemy_rect):
                if self.shield_active:
                    self.shield_active = False
                    enemy["y"] = random.randint(-220, -80)
                    return

                if self.crash_sound:
                    self.crash_sound.play()
                self.stop_bgm()
                self.state = SceneState.GAME_OVER
                self.high_score = self.save_system.update_highscore(self.score)
                return

    def move_lines(self):
        for line in self.lines:
            line.y += self.line_speed
            if line.y > DISPLAY.height:
                line.y = -90

    def draw_base_scene(self):
        self.screen.fill(self.current_theme.bg_color)
        pygame.draw.rect(self.screen, self.current_theme.edge_color, [0, 0, 50, DISPLAY.height])
        pygame.draw.rect(self.screen, self.current_theme.edge_color, [DISPLAY.width - 50, 0, 50, DISPLAY.height])
        pygame.draw.rect(self.screen, self.current_theme.road_color, [50, 0, DISPLAY.width - 100, DISPLAY.height])
        for line in self.lines:
            pygame.draw.rect(self.screen, self.current_theme.line_color, line)

    def draw_hud(self):
        score_text = self.font.render(f"Score: {self.score}", True, WHITE)
        high_text = self.font.render(f"Best: {self.high_score}", True, WHITE)
        boost_text = self.font.render(
            f"Boost: {'READY' if self.boost_cooldown == 0 else self.boost_cooldown // DISPLAY.fps + 1}s",
            True,
            WHITE,
        )
        vehicle = self.vehicles[self.selected_vehicle_idx]
        vehicle_text = self.font.render(f"차량: {vehicle.name}", True, vehicle.color)
        mission_color = (120, 255, 150) if self.mission["completed"] else ((255, 120, 120) if self.mission["failed"] else WHITE)
        mission_text = self.font.render(self.mission["label"], True, mission_color)
        mission_prog = self.font.render(
            f"{min(self.frame_counter // DISPLAY.fps, self.mission['target_frames'] // DISPLAY.fps)} / {self.mission['target_frames'] // DISPLAY.fps}s",
            True,
            mission_color,
        )
        theme_text = self.font.render(f"트랙: {self.current_theme.name}", True, WHITE)

        self.screen.blit(score_text, (10, 8))
        self.screen.blit(high_text, (10, 32))
        self.screen.blit(boost_text, (10, 56))
        self.screen.blit(vehicle_text, (10, 80))
        self.screen.blit(theme_text, (10, 104))
        self.screen.blit(mission_text, (10, 128))
        self.screen.blit(mission_prog, (10, 152))

        if self.shield_active:
            shield_text = self.font.render("Shield: ON", True, (80, 220, 255))
            self.screen.blit(shield_text, (DISPLAY.width - 145, 10))

    def draw_items(self):
        if self.shield_item:
            pygame.draw.ellipse(self.screen, (80, 220, 255), self.shield_item)

        if self.shield_active:
            aura_rect = pygame.Rect(self.car_x - 5, self.car_y - 5, 60, 100)
            pygame.draw.ellipse(self.screen, (80, 220, 255), aura_rect, 3)

    def draw_mobile_controls(self):
        left_btn, right_btn, boost_btn = self.mobile_button_rects()
        pygame.draw.rect(self.screen, (80, 80, 80), left_btn, border_radius=8)
        pygame.draw.rect(self.screen, (80, 80, 80), right_btn, border_radius=8)
        pygame.draw.rect(self.screen, (90, 60, 120), boost_btn, border_radius=8)
        self.screen.blit(self.font.render("◀", True, WHITE), (left_btn.x + 17, left_btn.y + 8))
        self.screen.blit(self.font.render("▶", True, WHITE), (right_btn.x + 17, right_btn.y + 8))
        self.screen.blit(self.font.render("BOOST", True, WHITE), (boost_btn.x + 7, boost_btn.y + 10))

    def mobile_button_rects(self):
        return (
            pygame.Rect(18, DISPLAY.height - 58, 48, 42),
            pygame.Rect(72, DISPLAY.height - 58, 48, 42),
            pygame.Rect(DISPLAY.width - 100, DISPLAY.height - 58, 82, 42),
        )

    def draw_overlay_text(self, message: str, sub_message: str):
        text1 = self.large_font.render(message, True, RED if "Game Over" in message else WHITE)
        text2 = self.font.render(sub_message, True, WHITE)
        self.screen.blit(text1, (DISPLAY.width // 2 - text1.get_width() // 2, DISPLAY.height // 2 - 80))
        self.screen.blit(text2, (DISPLAY.width // 2 - text2.get_width() // 2, DISPLAY.height // 2 - 42))

    def draw_start_menu(self):
        self.draw_overlay_text("SPACE: Start", "1/2/3 으로 차량 선택")
        y = DISPLAY.height // 2
        for idx, vehicle in enumerate(self.vehicles, start=1):
            selected = "<" if idx - 1 == self.selected_vehicle_idx else " "
            line = f"{selected} {idx}. {vehicle.name}  가속:{vehicle.acceleration:.2f} 최고속:{vehicle.max_speed:.1f} 핸들링:{vehicle.handling:.2f}"
            txt = self.font.render(line, True, vehicle.color)
            self.screen.blit(txt, (16, y + idx * 24))

    def render(self):
        self.draw_base_scene()
        self.draw_items()
        for enemy in self.enemies:
            self.screen.blit(self.obs_img, (enemy["x"], enemy["y"]))
        self.screen.blit(self.car_img, (self.car_x, self.car_y))
        self.draw_hud()
        self.draw_mobile_controls()

        if self.state == SceneState.START:
            self.draw_start_menu()
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
