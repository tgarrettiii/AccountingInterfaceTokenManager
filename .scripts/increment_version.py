import os
import subprocess
import sys

def run_cmd(command):
    """Helper to run system commands safely and capture output."""
    try:
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        return result.stdout.strip()
    except subprocess.CalledProcessError:
        # Return empty if the git command fails (e.g., no tags exist yet)
        return ""

def get_next_tag():
    # 1. Fetch all tags from the remote to make sure the runner has them
    run_cmd(['git', 'fetch', '--tags', '--force'])

    # 2. Get the latest tag matching the 'v*' pattern across the repo history
    latest_tag = run_cmd(['git', 'describe', '--tags', '--abbrev=0', '--match', 'v*'])

    # Fallback if no tags exist in the repo yet
    if not latest_tag:
        print("--> [INFO] No matching 'v*' tags found in repository history.")
        print("--> [INFO] Falling back to default version: 1.0.0")
        return "1.0.0"

    print(f"Latest found tag: {latest_tag}")

    try:
        # Strip the 'v' prefix to isolate the raw digits (e.g., 'v1.2.3' -> '1.2.3')
        clean_version = latest_tag.lstrip('v')
        
        # Split into SemVer components
        parts = clean_version.split('.')
        
        if len(parts) < 3:
            raise ValueError("Tag does not follow a Major.Minor.Patch format (e.g., v1.0.0)")

        # Extract components and increment the patch version (last position) by 1
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
    # Calculate the version
    next_ver = get_next_tag()
    print(f"Calculated next version: {next_ver}")
    
    # Export the variable as 'VERSION' to GitHub Actions environment
    if "GITHUB_OUTPUT" in os.environ:
        with open(os.environ["GITHUB_OUTPUT"], "a") as f:
            f.write(f"VERSION={next_ver}\n")
        print("--> [SUCCESS] Successfully exported 'VERSION' to GITHUB_OUTPUT.")
    else:
        print("--> [WARNING] GITHUB_OUTPUT environment variable not found.")
