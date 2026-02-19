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
    initial_obstacle_speed: float = 5.0
    obstacle_speed_increase: float = 0.2
    initial_line_speed: float = 5.0
    line_speed_increase: float = 0.05


@dataclass(frozen=True)
class AssetConfig:
    car_image: str = "car.png"
    obstacle_image: str = "obstacle.png"
    bgm: str = "bgm.mp3"
    crash_sfx: str = "crash.wav"


DISPLAY = DisplayConfig()
BALANCE = BalanceConfig()
ASSETS = AssetConfig()

WHITE = (255, 255, 255)
GRAY = (60, 60, 60)
RED = (200, 30, 30)
YELLOW = (255, 230, 50)
ROAD_EDGE = (40, 40, 40)
