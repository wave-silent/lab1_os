#!/usr/bin/env python3
# Импорт необходимых библиотек
import os           # для работы с операционной системой (загрузка, пути и т.д.)
import platform     # для получения информации о платформе и системе
import subprocess   # для запуска внешних команд (например lsb_release)
import socket       # для работы с сетевыми функциями (hostname)
import getpass      # для получения информации о пользователе
import psutil       # для получения системной информации (память, диски, процессоры)

def get_system_info():
    
    # Получение информации о дистрибутиве через lsb_release
    try:
        # Запускаем команду lsb_release -d и получаем её вывод
        result = subprocess.run(['lsb_release', '-d'], capture_output=True, text=True, check=True)
        # Извлекаем название дистрибутива из вывода команды
        distro = result.stdout.split(':')[1].strip()
        print(f"OS: {distro}")
    except:
        # Если lsb_release не работает, читаем файл /etc/os-release
        try:
            with open('/etc/os-release', 'r') as f:
                for line in f:
                    # Ищем строку с названием дистрибутива
                    if line.startswith('PRETTY_NAME='):
                        distro = line.split('=')[1].strip().strip('"')
                        print(f"OS: {distro}")
                        break
        except:
            print("OS: Unknown")
    
    # Получение версии ядра через platform (аналог uname в Python)
    print(f"Kernel: {platform.system()} {platform.release()}")
    
    # Получение архитектуры процессора
    print(f"Architecture: {platform.machine()}")
    
    # Получение hostname и имени пользователя
    print(f"Hostname: {socket.gethostname()}")
    print(f"User: {getpass.getuser()}")
    
    # Получение информации об оперативной памяти через psutil
    memory = psutil.virtual_memory()
    # Перевод байтов в мегабайты и вывод
    print(f"RAM: {memory.available // 1024 // 1024}MB free / {memory.total // 1024 // 1024}MB total")
    
    # Получение информации о swap-памяти
    swap = psutil.swap_memory()
    print(f"Swap: {swap.total // 1024 // 1024}MB total / {swap.free // 1024 // 1024}MB free")

    # Получение виртуальной памяти через чтение /proc/meminfo
    try:
        with open('/proc/meminfo', 'r') as f:
            for line in f:
                # Ищем строку с общей виртуальной памятью
                if line.startswith('VmallocTotal'):
                    vmalloc = int(line.split()[1])
                    # Перевод килобайтов в мегабайты
                    print(f"Virtual memory: {vmalloc // 1024} MB")
                    break
    except:
        print("Virtual memory: Information not available")
   # Получение количества логических процессоров
    print(f"Processors: {psutil.cpu_count(logical=True)}")
    
    # Получение средней загрузки системы
    load_avg = os.getloadavg()
    #load_avg[0] - средняя загрузка за 1 минуту
    #load_avg[1] - средняя загрузка за 5 минут
    #load_avg[2] - средняя загрузка за 15 минут
    #2f - два знака после запятой
    print(f"Load average: {load_avg[0]:.2f}, {load_avg[1]:.2f}, {load_avg[2]:.2f}")
    
    # Получение информации о подключенных дисках
    print("Drives:")
    for partition in psutil.disk_partitions():
        # Пропускаем системные файловые системы и snap-пакеты
        #Почему мы это делаем?
        #Чтобы убрать из вывода служебные файловые системы, которые не являются реальными дисками.
        if partition.fstype in ['proc', 'sysfs', 'devtmpfs', 'devpts', 'tmpfs', 'cgroup', 'squashfs']:
            continue
        if any(partition.mountpoint.startswith(x) for x in ['/proc', '/sys', '/dev', '/snap']):
            continue
            
        try:
            # Получаем статистику использования диска
            usage = psutil.disk_usage(partition.mountpoint)
            if usage.total > 0:
                # Перевод байтов в гигабайты
                free_gb = usage.free // (1024**3)
                total_gb = usage.total // (1024**3)
                print(f"  {partition.mountpoint:10} {partition.fstype:8} {free_gb}GB free / {total_gb}GB total")
        except:
            continue

# Вызов главной функции
get_system_info()
