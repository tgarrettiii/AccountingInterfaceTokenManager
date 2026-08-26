import subprocess
import sys

def run_cmd(command):
    """Helper to run system commands safely."""
    try:
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        return result.stdout.strip()
    except subprocess.CalledProcessError as e:
        # Return empty if the git command fails (e.g., no tags exist yet)
        return ""

def get_next_tag():
    # 1. Fetch all tags from the remote to make sure we aren't missing any
    run_cmd(['git', 'fetch', '--tags'])

    # 2. Get the latest tag matching the 'v*' pattern sorted by version order
    # --merged HEAD ensures we only look at tags reachable from this branch
    latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*', '--merged', 'HEAD'])

    # Fallback if no tags exist in the repo yet
    if not latest_tag:
        print("No matching tags found. Defaulting to: v1.0.0")
        return "1.0.0"

    print(f"Latest found tag: {latest_tag}")

    try:
        # Remove the 'v' prefix if it exists
        clean_version = latest_tag.lstrip('v')

        # Split the version components (e.g., '1.2.3' -> ['1', '2', '3'])
        parts = clean_version.split('.')

        if len(parts) < 3:
            raise ValueError("Tag format does not match Major.Minor.Patch structure.")

        # Parse and increment the last position (Patch version)
        major = parts[0]
        minor = parts[1]
        patch = int(parts[2]) + 1

        new_version = f"{major}.{minor}.{patch}"
        return new_version

    except Exception as e:
        print(f"Error parsing tag '{latest_tag}': {e}. Defaulting to fallback.", file=sys.stderr)
        return "1.0.0"

if __name__ == "__main__":
    next_ver = get_next_tag()
    print(f"Calculated next version: {next_ver}")

    # Write to GitHub Output so the runner can use this variable in later steps
    # Running this print statement writes directly into the runner environment
    print(f"next_version={next_ver} >> $GITHUB_OUTPUT")
