import os, json, time
import redis
import requests
from minio import Minio

REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))

MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "localhost:9000")
MINIO_ACCESS_KEY = os.getenv("MINIO_ACCESS_KEY", "minioadmin")
MINIO_SECRET_KEY = os.getenv("MINIO_SECRET_KEY", "minioadmin")
MINIO_BUCKET = os.getenv("MINIO_BUCKET", "vto")

API_BASE = os.getenv("API_BASE", "http://localhost:8080")
WORKER_TOKEN = os.getenv("WORKER_TOKEN", "dev-worker-token")

r = redis.Redis(host=REDIS_HOST, port=REDIS_PORT, decode_responses=True)
m = Minio(MINIO_ENDPOINT, access_key=MINIO_ACCESS_KEY, secret_key=MINIO_SECRET_KEY, secure=False)

def list_session_frames(session_id: str):
    prefix = f"raw/{session_id}/"
    objs = m.list_objects(MINIO_BUCKET, prefix=prefix, recursive=True)
    keys = [o.object_name for o in objs]
    keys.sort()
    return keys

def notify_processed(session_id: str, frame_keys):
    url = f"{API_BASE}/internal/capture-sessions/{session_id}/processed"
    headers = {"X-Worker-Token": WORKER_TOKEN, "Content-Type": "application/json"}
    payload = {"frameKeys": frame_keys}
    resp = requests.post(url, headers=headers, data=json.dumps(payload), timeout=15)
    resp.raise_for_status()

print("[worker] started. waiting jobs:capture ...")

while True:
    try:
        item = r.blpop("jobs:capture", timeout=0)
        _, job_json = item
        job = json.loads(job_json)
        session_id = job["sessionId"]

        print(f"[worker] job received session={session_id}")
        frame_keys = list_session_frames(session_id)

        if not frame_keys:
            print(f"[worker] no frames yet. requeue session={session_id}")
            time.sleep(2)
            r.rpush("jobs:capture", job_json)
            continue

        notify_processed(session_id, frame_keys)
        print(f"[worker] processed session={session_id}, frames={len(frame_keys)}")

    except Exception as e:
        print("[worker] error:", e)
        time.sleep(2)
