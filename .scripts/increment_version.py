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
    print(f"--> [INFO] Target branch context: {target_branch}")

    # 1. FORCE FIRST PRIORITY: Scan strictly the remote tracking branch of the target branch (main)
    # This prevents the script from accidentally picking up incoming feature branch tags.
    latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*', f'origin/{target_branch}'])
    
    if latest_tag:
        print(f"--> [SUCCESS] Found primary tag on {target_branch} line: {latest_tag}")
    else:
        print(f"--> [WARNING] No v* tags found directly on origin/{target_branch}. Trying local reference...")
        latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*', target_branch])

    # 2. EMERGENCY FALLBACK: Only if both direct checks fail, look through global tags
    if not latest_tag:
        print("--> [FALLBACK] No direct tags found. Scanning all repository tags...")
        tags_string = run_cmd(['git', 'tag', '-l', 'v*', '--sort=-v:refname'])
        
        if tags_string:
            tags_list = [t.strip() for t in tags_string.split('\n') if t.strip()]
            for tag in tags_list:
                is_merged_local = run_cmd(['git', 'merge-base', '--is-ancestor', tag, target_branch]) == ""
                is_merged_remote = run_cmd(['git', 'merge-base', '--is-ancestor', tag, f'origin/{target_branch}']) == ""
                if is_merged_local or is_merged_remote:
                    latest_tag = tag
                    print(f"--> [FALLBACK SUCCESS] Identified fallback tag in branch history: {latest_tag}")
                    break

    # 3. ABSOLUTE CRASH FALLBACK: If nothing exists anywhere in the repository history
    if not latest_tag:
        print("--> [INFO] No tags found anywhere in history. Defaulting to base version.")
        return "1.0.0"

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
        return "1.0.0"

if __name__ == "__main__":
    target_br = "main"
    if len(sys.argv) > 1 and sys.argv[1].strip():
        target_br = sys.argv[1].strip()

    next_ver = get_next_tag(target_br)
    print(f"Calculated next version: {next_ver}")

    if "GITHUB_OUTPUT" in os.environ:
        with open(os.environ["GITHUB_OUTPUT"], "a") as f:
            f.write(f"VERSION={next_ver}\n")
