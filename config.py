"""게임 설정 모듈."""

from dataclasses import dataclass


@dataclass(frozen=True)
class DisplayConfig:
    width: int = 400
    height: int = 600
    fps: int = 60
    title: str = "🚗 자동차 경주 (Advanced Edition)"


@dataclass(frozen=True)
class BalanceConfig:
    max_player_speed: float = 9.0
    lane_change_speed: float = 0.15
    boost_extra_speed: float = 4.0
    boost_duration_frames: int = 40
    boost_cooldown_frames: int = 240
    initial_enemy_speed: float = 4.2
    enemy_speed_increase: float = 0.12
    initial_line_speed: float = 5.0
    line_speed_increase: float = 0.04
    score_tick_frames: int = 60
    lane_count: int = 3
    enemy_count: int = 3


@dataclass(frozen=True)
class ItemConfig:
    shield_spawn_interval_frames: int = 360
    shield_fall_speed: float = 4.0
    shield_size: tuple[int, int] = (28, 28)


@dataclass(frozen=True)
class SaveConfig:
    highscore_file: str = "highscore.json"


@dataclass(frozen=True)
class AssetConfig:
    car_image: str = "car.png"
    obstacle_image: str = "obstacle.png"
    bgm: str = "bgm.mp3"
    crash_sfx: str = "crash.wav"


DISPLAY = DisplayConfig()
BALANCE = BalanceConfig()
ITEMS = ItemConfig()
SAVE = SaveConfig()
ASSETS = AssetConfig()

WHITE = (255, 255, 255)
GRAY = (60, 60, 60)
RED = (200, 30, 30)
YELLOW = (255, 230, 50)
ROAD_EDGE = (40, 40, 40)
BLACK = (15, 15, 15)
