
import os           
import platform    
import subprocess   
import socket       
import getpass      
import psutil      

def get_system_info():
    
   
    try:
      
        result = subprocess.run(['lsb_release', '-d'], capture_output=True, text=True, check=True)
    
        distro = result.stdout.split(':')[1].strip()
        print(f"OS: {distro}")
    except:
     
        try:
            with open('/etc/os-release', 'r') as f:
                for line in f:
                   
                    if line.startswith('PRETTY_NAME='):
                        distro = line.split('=')[1].strip().strip('"')
                        print(f"OS: {distro}")
                        break
        except:
            print("OS: Unknown")
    
    
    print(f"Kernel: {platform.system()} {platform.release()}")
    
 
    print(f"Architecture: {platform.machine()}")
    
 
    print(f"Hostname: {socket.gethostname()}")
    print(f"User: {getpass.getuser()}")
    
    
    memory = psutil.virtual_memory()
  
    print(f"RAM: {memory.available // 1024 // 1024}MB free / {memory.total // 1024 // 1024}MB total")
    
   
    swap = psutil.swap_memory()
    print(f"Swap: {swap.total // 1024 // 1024}MB total / {swap.free // 1024 // 1024}MB free")

    
    try:
        with open('/proc/meminfo', 'r') as f:
            for line in f:
              
                if line.startswith('VmallocTotal'):
                    vmalloc = int(line.split()[1])
                  
                    print(f"Virtual memory: {vmalloc // 1024} MB")
                    break
    except:
        print("Virtual memory: Information not available")
 
    print(f"Processors: {psutil.cpu_count(logical=True)}")
    
   
    load_avg = os.getloadavg()
  

    print(f"Load average: {load_avg[0]:.2f}, {load_avg[1]:.2f}, {load_avg[2]:.2f}")
    
   
    print("Drives:")
    for partition in psutil.disk_partitions():
       
        if partition.fstype in ['proc', 'sysfs', 'devtmpfs', 'devpts', 'tmpfs', 'cgroup', 'squashfs']:
            continue
        if any(partition.mountpoint.startswith(x) for x in ['/proc', '/sys', '/dev', '/snap']):
            continue
            
        try:
         
            usage = psutil.disk_usage(partition.mountpoint)
            if usage.total > 0:
               
                free_gb = usage.free // (1024**3)
                total_gb = usage.total // (1024**3)
                print(f"  {partition.mountpoint:10} {partition.fstype:8} {free_gb}GB free / {total_gb}GB total")
        except:
            continue


get_system_info()
