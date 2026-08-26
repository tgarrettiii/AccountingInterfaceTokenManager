import os
import subprocess
import sys

def run_cmd(command):
    """Helper to run system commands safely and capture output."""
    try:
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        return result.stdout.strip()
    except subprocess.CalledProcessError:
        return ""

def get_next_tag(target_branch):
    print(f"--> [INFO] Searching for latest tag strictly merged into branch: {target_branch}")

    # Search local reference first, then fallback to remote tracker
    latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*', '--merged', target_branch])
    if not latest_tag:
        latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*'])

    # Fallback if no tags exist in that branch's history yet
    if not latest_tag:
        print(f"--> [INFO] No matching 'v*' tags found in {target_branch} or origin/{target_branch} history.")
        print("--> [INFO] Falling back to default version: 1.0.0")
        return "1.0.0"

    print(f"Latest found tag on target branch: {latest_tag}")

    try:
        clean_version = latest_tag.lstrip('v')
        parts = clean_version.split('.')
        if len(parts) < 3:
            raise ValueError("Tag does not follow a Major.Minor.Patch format (e.g., v1.0.0)")

        major = parts[0]
        minor = parts[1]
        patch = int(parts[2]) + 1

        new_version = f"{major}.{minor}.{patch}"
        return new_version
    except Exception as e:
        print(f"--> [ERROR] Failed parsing tag '{latest_tag}': {e}", file=sys.stderr)
        print("--> [INFO] Falling back to default version: 1.0.0")
        return "1.0.0"

if __name__ == "__main__":
    target_br = "main"
    if len(sys.argv) > 1 and sys.argv[1].strip():
        target_br = sys.argv[1].strip()

    next_ver = get_next_tag(target_br)
    print(f"Calculated next version: {next_ver}")

    # Export variable using UPPERCASE 'VERSION' for modern GitHub standards
    if "GITHUB_OUTPUT" in os.environ:
        with open(os.environ["GITHUB_OUTPUT"], "a") as f:
            f.write(f"VERSION={next_ver}\n")
        print("--> [SUCCESS] Successfully exported 'VERSION' to GITHUB_OUTPUT.")
    else:
        print("--> [WARNING] GITHUB_OUTPUT environment variable not found.")
