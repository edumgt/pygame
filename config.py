"""게임 설정 모듈."""

from dataclasses import dataclass


@dataclass(frozen=True)
class DisplayConfig:
    width: int = 400
    height: int = 600
    fps: int = 60
    title: str = "🚗 자동차 경주 (Scene Manager 적용)"


@dataclass(frozen=True)
class BalanceConfig:
    car_speed: int = 5
    boost_speed: int = 11
    boost_duration_frames: int = 45
    boost_cooldown_frames: int = 240
    initial_obstacle_speed: float = 5.0
    obstacle_speed_increase: float = 0.2
    initial_line_speed: float = 5.0
    line_speed_increase: float = 0.05
    score_tick_frames: int = 60


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
