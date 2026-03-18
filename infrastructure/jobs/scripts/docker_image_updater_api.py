import os
import sys
import docker
from flask import Flask, request, jsonify
from datetime import datetime

app = Flask(__name__)
client = docker.from_env()

API_TOKEN = os.environ.get("DOCKER_UPDATER_HTTP_API_TOKEN", "fallback")
LABEL_ENABLE = "com.touir.expensesmanager.dockerupdater.enable"
LABEL_SERVICE_NAME = "com.touir.expensesmanager.dockerupdater.servicename"
API_PORT = os.environ.get("DOCKER_UPDATER_HTTP_API_PORT", "8989")

def log(msg):
    print(f"[{datetime.now().isoformat()}] {msg}", flush=True)

def check_auth():
    auth_header = request.headers.get("Authorization")
    if not auth_header or auth_header != f"Bearer {API_TOKEN}":
        return False
    return True

def recreate_container(c, new_image_tag):
    info = c.attrs
    
    name = info['Name'].lstrip('/')
    image = new_image_tag
    command = info.get('Config', {}).get('Cmd')
    entrypoint = info.get('Config', {}).get('Entrypoint')
    environment = info.get('Config', {}).get('Env')
    labels = info.get('Config', {}).get('Labels')
    
    host_config = info.get('HostConfig', {})
    network_mode = host_config.get('NetworkMode')
    ports = host_config.get('PortBindings', {})
    
    mapped_ports = {}
    if ports:
        for k, v in ports.items():
            if v and isinstance(v, list):
                mapped_ports[k] = v[0].get('HostPort')
            
    volumes = host_config.get('Binds', [])
    restart_policy = host_config.get('RestartPolicy', {'Name': 'no', 'MaximumRetryCount': 0})
    
    network_settings = info.get('NetworkSettings', {})
    networks = network_settings.get('Networks', {})
    
    if info.get('State', {}).get('Status') == 'running':
        log(f"Stopping container {name}...")
        c.stop()
    else:
        log(f"Container {name} is already stopped, skipping stop...")
    log(f"Removing container {name}...")
    c.remove()
    
    log(f"Starting new container {name} with image {image}...")
    new_c = client.containers.run(
        image=image,
        name=name,
        command=command,
        entrypoint=entrypoint,
        environment=environment,
        labels=labels,
        ports=mapped_ports,
        volumes=volumes,
        network=network_mode,
        restart_policy=restart_policy,
        detach=True
    )
    
    # Attach secondary networks if any
    for net_name, net_config in networks.items():
        if net_name != network_mode:
            net = client.networks.get(net_name)
            ipv4 = net_config.get('IPAddress')
            aliases = net_config.get('Aliases')
            
            # Reattach but dropping the container ID alias auto-assigned by docker
            if aliases:
                aliases = [a for a in aliases if not a.startswith(info['Id'][:12])]
                
            net.connect(new_c, ipv4_address=ipv4 if ipv4 else None, aliases=aliases if aliases else None)

    log(f"Successfully recreated {name} (ID: {new_c.short_id})")

@app.route('/v1/update', methods=['GET'])
def update():
    if not check_auth():
        return jsonify({"error": "Unauthorized"}), 401
    
    container_name = request.args.get('container')
    service_name = request.args.get('servicename')
    
    updated_containers = []
    
    containers = client.containers.list(all=True)
    for c in containers:
        labels = c.labels
        if labels.get(LABEL_ENABLE) == "true" or labels.get("autoupdate") == "true":
            if container_name and c.name != container_name:
                continue
            
            if service_name and labels.get(LABEL_SERVICE_NAME) != service_name:
                continue
            
            image_tags = c.image.tags
            if not image_tags:
                image_tags = [c.attrs['Config']['Image']]
                
            tag_to_pull = image_tags[0]
                
            log(f"Checking updates for {c.name} ({tag_to_pull})")
            
            try:
                old_image_id = c.image.id
                client.images.pull(tag_to_pull)
                
                new_image = client.images.get(tag_to_pull)
                if new_image.id != old_image_id:
                    log(f"New image found! Updating {c.name}...")
                    recreate_container(c, tag_to_pull)
                    updated_containers.append(c.name)
                else:
                    log(f"No update required for {c.name}, images match.")
            except Exception as e:
                log(f"Error checking/updating {c.name}: {e}")
                
    return jsonify({"success": True, "updated": updated_containers})

if __name__ == '__main__':
    log(f"Starting Custom Updater API on port {API_PORT}...")
    app.run(host='0.0.0.0', port=API_PORT)
