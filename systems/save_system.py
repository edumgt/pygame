"""최고 점수 저장/불러오기 시스템."""

import json
import os


class SaveSystem:
    def __init__(self, highscore_file: str):
        self.highscore_file = highscore_file

    def load_highscore(self) -> int:
        if not os.path.exists(self.highscore_file):
            return 0

        try:
            with open(self.highscore_file, "r", encoding="utf-8") as file:
                data = json.load(file)
                return int(data.get("highscore", 0))
        except (OSError, ValueError, json.JSONDecodeError):
            return 0

    def update_highscore(self, score: int) -> int:
        highscore = self.load_highscore()
        if score <= highscore:
            return highscore

        try:
            with open(self.highscore_file, "w", encoding="utf-8") as file:
                json.dump({"highscore": score}, file)
        except OSError:
            return highscore

        return score
