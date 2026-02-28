#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
SOURCE_SCRIPTS_DIR="${REPO_ROOT}/UnityCarRacing/Assets/Scripts"

MODE="editor"
SYNC_SCRIPTS=1
PROJECT_PATH=""
UNITY_BIN="${UNITY_EDITOR:-}"
DEFAULT_PROJECT_PATH="${REPO_ROOT}/UnityProject"

usage() {
  cat <<'EOF'
Usage:
  scripts/unity/run_unity_game.sh [--project /abs/or/relative/UnityProject] [options]

Options:
  --project PATH      Unity project directory (optional)
  --unity PATH        Explicit Unity executable path
  --batch-check       Run batchmode compile check and exit
  --no-sync           Do not copy repo C# scripts into project Assets/Scripts
  -h, --help          Show help

Examples:
  scripts/unity/run_unity_game.sh --project ./UnityProject
  scripts/unity/run_unity_game.sh --project ./UnityProject --batch-check
  scripts/unity/run_unity_game.sh --project ./UnityProject --unity "/opt/unityhub/Editor/2022.3.62f1/Editor/Unity"
EOF
}

detect_project_path() {
  if [[ -d "$PWD/Assets" && -d "$PWD/ProjectSettings" ]]; then
    echo "$PWD"
    return 0
  fi

  if [[ -d "$DEFAULT_PROJECT_PATH/Assets" && -d "$DEFAULT_PROJECT_PATH/ProjectSettings" ]]; then
    echo "$DEFAULT_PROJECT_PATH"
    return 0
  fi

  local candidates=()
  while IFS= read -r line; do
    candidates+=("$line")
  done < <(find "$REPO_ROOT" -maxdepth 4 -type d -name ProjectSettings -printf '%h\n' 2>/dev/null | sort -u)

  if [[ "${#candidates[@]}" -eq 1 ]]; then
    echo "${candidates[0]}"
    return 0
  fi

  return 1
}

abs_path() {
  local input_path="$1"
  local parent

  if [[ "$input_path" = /* ]]; then
    echo "$input_path"
    return 0
  fi

  if [[ -d "$input_path" ]]; then
    (cd "$input_path" && pwd)
    return 0
  fi

  parent="$(dirname "$input_path")"
  if [[ "$parent" == "." ]]; then
    echo "$PWD/$(basename "$input_path")"
    return 0
  fi

  if [[ -d "$parent" ]]; then
    echo "$(cd "$parent" && pwd)/$(basename "$input_path")"
    return 0
  fi

  echo "$PWD/$input_path"
}

is_wsl() {
  grep -qiE '(microsoft|wsl)' /proc/sys/kernel/osrelease 2>/dev/null
}

detect_unity() {
  if [[ -n "$UNITY_BIN" ]]; then
    return
  fi

  if command -v unity-editor >/dev/null 2>&1; then
    UNITY_BIN="$(command -v unity-editor)"
    return
  fi

  local linux_unity=""
  local windows_unity=""

  linux_unity="$(ls -1d /opt/unityhub/Editor/*/Editor/Unity 2>/dev/null | sort -V | tail -n 1 || true)"
  windows_unity="$(ls -1d "/mnt/c/Program Files/Unity/Hub/Editor"/*/Editor/Unity.exe 2>/dev/null | sort -V | tail -n 1 || true)"

  if [[ -n "$linux_unity" ]]; then
    UNITY_BIN="$linux_unity"
    return
  fi

  if [[ -n "$windows_unity" ]]; then
    UNITY_BIN="$windows_unity"
    return
  fi
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --project)
      PROJECT_PATH="${2:-}"
      shift 2
      ;;
    --unity)
      UNITY_BIN="${2:-}"
      shift 2
      ;;
    --batch-check)
      MODE="batch"
      shift
      ;;
    --no-sync)
      SYNC_SCRIPTS=0
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "$PROJECT_PATH" ]]; then
  if PROJECT_PATH="$(detect_project_path)"; then
    echo "Detected Unity project: $PROJECT_PATH"
  else
    PROJECT_PATH="$DEFAULT_PROJECT_PATH"
    echo "No Unity project detected. Using default project path: $PROJECT_PATH"
  fi
fi

PROJECT_PATH="$(abs_path "$PROJECT_PATH")"

if [[ "$PROJECT_PATH" == *"<"* || "$PROJECT_PATH" == *">"* ]]; then
  cat <<'EOF' >&2
Invalid --project path: placeholder text detected.
Replace <...> with a real directory path.
Example:
  ./scripts/unity/run_unity_game.sh --project "/mnt/c/Users/1/Documents/UnityProjects/MyRacingGame"
EOF
  exit 1
fi

if [[ ! -d "$SOURCE_SCRIPTS_DIR" ]]; then
  echo "Source script directory not found: $SOURCE_SCRIPTS_DIR" >&2
  exit 1
fi

detect_unity

if [[ -z "$UNITY_BIN" ]]; then
  cat <<EOF >&2
Unity executable not found.
Try one of the following:
1) Install Unity via Unity Hub
2) Pass executable path explicitly:
   scripts/unity/run_unity_game.sh --project "$PROJECT_PATH" --unity "/path/to/Unity"
3) Export UNITY_EDITOR:
   export UNITY_EDITOR="/path/to/Unity"
EOF
  exit 1
fi

if [[ ! -f "$UNITY_BIN" && ! -x "$UNITY_BIN" ]]; then
  echo "Unity executable not accessible: $UNITY_BIN" >&2
  exit 1
fi

PROJECT_ARG="$PROJECT_PATH"
if [[ "$UNITY_BIN" == *.exe ]]; then
  if is_wsl; then
    PROJECT_ARG="$(wslpath -w "$PROJECT_PATH")"
  else
    echo "Windows Unity executable detected outside WSL: $UNITY_BIN" >&2
    exit 1
  fi
fi

if [[ "$MODE" == "batch" ]]; then
  if [[ ! -d "$PROJECT_PATH/Assets" || ! -d "$PROJECT_PATH/ProjectSettings" ]]; then
    cat <<EOF >&2
Cannot run --batch-check because project is not initialized:
  $PROJECT_PATH
Run editor mode once first:
  ./scripts/unity/run_unity_game.sh --project "$PROJECT_PATH"
EOF
    exit 1
  fi

  if [[ "$SYNC_SCRIPTS" -eq 1 ]]; then
    mkdir -p "$PROJECT_PATH/Assets/Scripts"
    cp -f "$SOURCE_SCRIPTS_DIR"/*.cs "$PROJECT_PATH/Assets/Scripts/"
    echo "Synced C# scripts to: $PROJECT_PATH/Assets/Scripts"
  fi

  echo "Running Unity batch check..."
  "$UNITY_BIN" -projectPath "$PROJECT_ARG" -quit -batchmode -logFile -
  exit $?
fi

if [[ ! -d "$PROJECT_PATH/Assets" || ! -d "$PROJECT_PATH/ProjectSettings" ]]; then
  echo "Unity project not initialized yet: $PROJECT_PATH"
  mkdir -p "$PROJECT_PATH/Assets/Scripts"
  if [[ "$SYNC_SCRIPTS" -eq 1 ]]; then
    cp -f "$SOURCE_SCRIPTS_DIR"/*.cs "$PROJECT_PATH/Assets/Scripts/"
    echo "Seeded C# scripts to: $PROJECT_PATH/Assets/Scripts"
  fi
  echo "Launching Unity Editor for first-time project initialization..."
  "$UNITY_BIN" -projectPath "$PROJECT_ARG"
  exit $?
fi

if [[ "$SYNC_SCRIPTS" -eq 1 ]]; then
  mkdir -p "$PROJECT_PATH/Assets/Scripts"
  cp -f "$SOURCE_SCRIPTS_DIR"/*.cs "$PROJECT_PATH/Assets/Scripts/"
  echo "Synced C# scripts to: $PROJECT_PATH/Assets/Scripts"
fi

echo "Launching Unity Editor..."
"$UNITY_BIN" -projectPath "$PROJECT_ARG"
