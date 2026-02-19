"""에셋 경로/로드를 일원화하는 리소스 로더."""

import os
import pygame


class ResourceLoader:
    def __init__(self, base_path: str = "."):
        self.base_path = base_path

    def _resolve(self, path: str) -> str:
        return os.path.join(self.base_path, path)

    def load_image(self, path: str, size: tuple[int, int], fallback_color: tuple[int, int, int]) -> pygame.Surface:
        full_path = self._resolve(path)
        if os.path.exists(full_path):
            image = pygame.image.load(full_path).convert_alpha()
            return pygame.transform.smoothscale(image, size)

        surface = pygame.Surface(size, pygame.SRCALPHA)
        surface.fill(fallback_color)
        return surface

    def load_sound(self, path: str, volume: float = 1.0):
        full_path = self._resolve(path)
        if not os.path.exists(full_path):
            return None

        sound = pygame.mixer.Sound(full_path)
        sound.set_volume(volume)
        return sound

    def load_bgm(self, path: str, volume: float = 0.5) -> bool:
        full_path = self._resolve(path)
        if not os.path.exists(full_path):
            return False

        pygame.mixer.music.load(full_path)
        pygame.mixer.music.set_volume(volume)
        return True
