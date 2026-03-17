import socket
import threading
import json
import sys
import os
from datetime import datetime

def log(msg):
    print(f"[{datetime.now().isoformat()}] {msg}", flush=True)

def forward(src, dst):
    try:
        while True:
            data = src.recv(4096)
            if not data: break
            dst.sendall(data)
    except Exception:
        pass
    finally:
        src.close()
        dst.close()

def handle_client(client_sock, target_host, target_port):
    target_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        target_sock.connect((target_host, target_port))
    except Exception as e:
        log(f"Failed to connect to {target_host}:{target_port} - {e}")
        client_sock.close()
        return
    
    threading.Thread(target=forward, args=(client_sock, target_sock), daemon=True).start()
    threading.Thread(target=forward, args=(target_sock, client_sock), daemon=True).start()

def setup_listener(source_host, source_port, target_host, target_port):
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
    try:
        server.bind((source_host, source_port))
    except Exception as e:
        log(f"Failed to bind to {source_host}:{source_port} - {e}")
        return
        
    server.listen(5)
    log(f"Proxy listening on {source_host}:{source_port} -> {target_host}:{target_port}")
    
    while True:
        try:
            client, _ = server.accept()
            threading.Thread(target=handle_client, args=(client, target_host, target_port), daemon=True).start()
        except KeyboardInterrupt:
            break
        except Exception as e:
            log(f"Error accepting connection on {source_port}: {e}")

def main():
    config_file = sys.argv[1] if len(sys.argv) > 1 else '/opt/jobs/scripts/proxy_config.json'
    
    if not os.path.exists(config_file):
        log(f"Config file not found: {config_file}")
        sys.exit(1)
        
    try:
        with open(config_file, 'r') as f:
            mappings = json.load(f)
    except Exception as e:
        log(f"Failed to read or parse {config_file}: {e}")
        sys.exit(1)
        
    threads = []
    for mapping in mappings:
        src_host = mapping.get('source_host', '127.0.0.1')
        src_port = mapping.get('source_port')
        tgt_host = mapping.get('target_host')
        tgt_port = mapping.get('target_port')
        
        if not all([src_port, tgt_host, tgt_port]):
            log(f"Invalid mapping definition: {mapping}")
            continue
            
        t = threading.Thread(target=setup_listener, args=(src_host, src_port, tgt_host, tgt_port), daemon=True)
        t.start()
        threads.append(t)
        
    # Wait indefinitely
    try:
        for t in threads:
            t.join()
    except KeyboardInterrupt:
        log("Proxy stopped")

if __name__ == '__main__':
    main()
