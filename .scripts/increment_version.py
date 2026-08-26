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
    # 1. Fetch all tags from the remote
    run_cmd(['git', 'fetch', '--tags', '--force'])
    
    # 2. Also ensure we have the target branch history tracked locally
    run_cmd(['git', 'fetch', 'origin', target_branch])

    print(f"--> [INFO] Searching for latest tag strictly merged into target branch: {target_branch}")

    # 3. CRITICAL FIX: Only look at tags reachable/merged into the target branch reference
    latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*', '--merged', f'origin/{target_branch}'])

    # Fallback if no tags exist in that branch's history yet
    if not latest_tag:
        print(f"--> [INFO] No matching 'v*' tags found in origin/{target_branch} history.")
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
    # Get the target branch from the command line argument, default to 'main' if missing
    target_br = sys.argv[1] if len(sys.argv) > 1 else "main"
    
    # Calculate the version based on that specific branch
    next_ver = get_next_tag(target_br)
    print(f"Calculated next version: {next_ver}")
    
    # Export the variable as 'VERSION' to GitHub Actions environment
    if "GITHUB_OUTPUT" in os.environ:
        with open(os.environ["GITHUB_OUTPUT"], "a") as f:
            f.write(f"VERSION={next_ver}\n")
        print("--> [SUCCESS] Successfully exported 'VERSION' to GITHUB_OUTPUT.")
    else:
        print("--> [WARNING] GITHUB_OUTPUT environment variable not found.")
