# NFS和FastDFS



查看系统版本

```
cat /etc/redhat-release
```

查看系统内核

```
uname -r
```

## 搭建NFS Server host

> 注意：共享系统的服务端和客户端不能安装在同一台服务器上

- 安装依赖

```shell
yum install rpcbind nfs-utils
y/d/n   y
```

- 分别为rpcbind和nfs服务设置开机启动服务

```shell
## 首先必须先启动RPC服务
systemctl start rpcbind && systemctl enable rpcbind
## 再启动NFS服务
systemctl start nfs-server && systemctl enable nfs-server
## 查看状态
systemctl status rpcbind
systemctl status nfs-server
```

- 创建共享文件夹

```shell
mkdir mydata
cd mydata
mkdir nfs
cd nfs
mkdir share
cd share
```

- 编辑nfs服务器配置文件

```shell
# 1. 编辑文件
vim /etc/exports

# 2. 新增如下内容
/mydata/nfs/share 192.168.3.*(rw,sync,insecure,no_subtree_check,no_root_squash)
# rw表示可读可写; no_root_squash的配置可以让任何用户都能访问此文件夹
## 使用该路径
/mydata/nfs/share *(rw,sync,insecure,no_subtree_check,no_root_squash)
-----------------------------------------------------------------------------
/home *(ro,sync,insecure,no_root_squash)
/data/nginx 192.168.1.*(rw,sync,insecure,no_subtree_check,no_root_squash)
参数  说明
ro  只读访问
rw  读写访问
sync    所有数据在请求时写入共享
async   nfs在写入数据前可以响应请求
secure  nfs通过1024以下的安全TCP/IP端口发送
insecure nfs通过1024以上的端口发送
wdelay  如果多个用户要写入nfs目录，则归组写入（默认）
no_wdelay   如果多个用户要写入nfs目录，则立即写入，当使用async时，无需此设置
hide    在nfs共享目录中不共享其子目录
no_hide 共享nfs目录的子目录
subtree_check   如果共享/usr/bin之类的子目录时，强制nfs检查父目录的权限（默认）
no_subtree_check    不检查父目录权限
all_squash  共享文件的UID和GID映射匿名用户anonymous，适合公用目录
no_all_squash   保留共享文件的UID和GID（默认）
root_squash root用户的所有请求映射成如anonymous用户一样的权限（默认）
no_root_squash  root用户具有根目录的完全管理访问权限
anonuid=xxx 指定nfs服务器/etc/passwd文件中匿名用户的UID
anongid=xxx 指定nfs服务器/etc/passwd文件中匿名用户的GID
```

- reload配置文件

```shell
exportfs -rv
```

- 关闭防火墙

```shell
systemctl stop firewalld.service  # 停止防火墙服务
systemctl disable firewalld.service # 禁止开机启动
```

- 查看是否开启

  ~~~shell
   netstat -aux | grep 111
   ## 或者
    netstat -lntp | grep 111
  ~~~

放行端口

~~~shell
firewall-cmd --add-port=111/tcp --permanent
firewall-cmd --reload
firewall-cmd --list-all
~~~

## 搭建NFS Client host

1. 安装**nfs-utils**依赖      **==不安装rpcbind==**

   ```shell
   yum install nfs-utils
   ```

2. 执行命令测试NFS Host是否配置了共享目录

   ```shell
   showmount -e 192.168.72.163
   ```

   ![image-20230120190955129](C:\Users\wangm\AppData\Roaming\Typora\typora-user-images\image-20230120190955129.png)

3. 将**NFS Client1**的`/root/testshare`目录挂载在NFS Host的`/share`目录上

   ```shell
   # 在NFS Client1 机器上执行如下命令
   # 创建/root/testshare文件夹
   mkdir /mydata
   cd mydata
   mkdir testshare
   cd testshare
   
   # 挂载目录, 命令规则: mount -t nfs NFS_HOST_IP:共享的目录 当前要挂载到NFSHOST共享目录的目录
   mount -t nfs 192.168.72.163:/mydata/nfs/share /mydata/testshare
   ```

4. 查看挂载结果

   ```shell
   df -h 
   ```

   ![image-20210601114903430](file:///D:/%E6%9C%9D%E5%A4%95/%E6%9E%B6%E6%9E%84%E7%8F%AD/DistributeTransaction%E5%88%86%E5%B8%83%E5%BC%8F/20210604Architect02Course016%E5%88%86%E5%B8%83%E5%BC%8F%E6%96%87%E4%BB%B6%E7%B3%BB%E7%BB%9F/%E8%AF%BE%E4%BB%B6/NFS%E5%92%8CFastDFS/img/image-20210601114903430.png)

## NFS服务功能测试

1. 在NFS Client1中的

   ```
   /mydata/testshare
   ```

   目录下执行如下命令

   ```shell
   echo "Hello NFS Client1 192.168.3.11" > nfsclient1.html
   ```

2. 在NFS HOST中的

   ```
   /mydata/nfs/share
   ```

   目录下执行命令

   ```shell
   ls && cat nfsclient1.html
   ```

   ![image-20210601115103030](file:///D:/%E6%9C%9D%E5%A4%95/%E6%9E%B6%E6%9E%84%E7%8F%AD/DistributeTransaction%E5%88%86%E5%B8%83%E5%BC%8F/20210604Architect02Course016%E5%88%86%E5%B8%83%E5%BC%8F%E6%96%87%E4%BB%B6%E7%B3%BB%E7%BB%9F/%E8%AF%BE%E4%BB%B6/NFS%E5%92%8CFastDFS/img/image-20210601115103030.png)若能如上图所示  NFS服务搭建成功

## 可能存在的问题

- 因为**mount**命令的挂载是临时的, 当我们重启机器后, **mount**命令就会失效。

  - **解决方案: 每次开机再挂载一遍**

  这里可以采用**添加自定义service**或者利用**rc.local**的方式实现

  > **方式一**
  >
  > 更改client服务器挂载文件
  >
  > vim /etc/fstab
  >
  > ```bash
  > tmpfs                /dev/shm         tmpfs    defaults          0 0
  > 
  > devpts               /dev/pts          devpts   gid=5,mode=620  0 0
  > 
  > sysfs                 /sys             sysfs    defaults          0 0
  > 
  > proc                 /proc            proc    defaults          0 0
  > 
  > 192.168.72.163:/mydata/nfs/share    /mydata/testshare          nfs      defaults          0 0
  > ```
  >
  > 以上，在最后一行，添加该挂载，保证重启后挂载仍然生效。
  >
  > **方式二**
  >
  > vim /etc/rc.local
  >
  > ```bash
  > [root@NFS_client mnt]# vi /etc/rc.local   
  > #!/bin/sh
  > touch /var/lock/subsys/local
  > /bin/mount -t nfs 192.168.72.163:/mydata/nfs/share /mydata/testshare
  > ```
  >
  > 添加文件执行权限
  >
  > ```bash
  > chmod +x /etc/rc.d/rc.local
  > ```
  >
  > ![image-20210601122345014](file:///D:/%E6%9C%9D%E5%A4%95/%E6%9E%B6%E6%9E%84%E7%8F%AD/DistributeTransaction%E5%88%86%E5%B8%83%E5%BC%8F/20210604Architect02Course016%E5%88%86%E5%B8%83%E5%BC%8F%E6%96%87%E4%BB%B6%E7%B3%BB%E7%BB%9F/%E8%AF%BE%E4%BB%B6/NFS%E5%92%8CFastDFS/img/image-20210601122345014.png)

- 若突然发现`/share`容量不够, 该怎么办?

  - 解决方案:
    - 加硬盘
    - 将其他磁盘中剩余的空间分出一个区并将`/share`文件夹挂载于此
  - 其实上面两种方案属于同一种, 即: `将/share文件夹挂载到容量大的磁盘中`



### 问题

- 单点故障问题
- 扩容问题

## Docker部署FastDFS

```bash
# 搜索镜像
docker search fastdfs

# 拉取镜像（已经内置Nginx）
docker pull delron/fastdfs

# 构建Tracker
# 22122 => Tracker默认端口
docker run --name=tracker-server --network=host --privileged=true -v /Demo/FastDFS/tracker:/var/fdfs -d delron/fastdfs tracker

# 构建Storage
# 23000 => Storage默认端口
# 8888 => 内置Nginx默认端口
# TRACKER_SERVER => 执行Tracker的ip和端口
# --net=host => 避免因为Docker网络问题导致外网客户端无法上传文件，因此使用host网络模式
docker run --name=storage-server --privileged=true --network=host -v /Demo/FastDFS/storage:/var/fdfs -e TRACKER_SERVER=134.175.91.184:22122 -e GROUP_NAME=group1 -d delron/fastdfs storage

 
##异常停止
 docker cp /root/storaged.log 3de619363d4e:/var/fdfs/logs/storaged.log
1.# 通过查看容器日志，确定是什么问题,docker logs -f -t --tail 行数 容器名
    docker logs -f -t --tail 100 <containerID> 
2. # 将容器中导致错误的文件拷贝到宿主机上
    docker cp containerID:container_path host_path 
    ***说明***
    	containerID：容器ID
    	container_path：容器内文件路径（需拷贝的源文件）
    	host_path：宿主机路径（拷贝文件的目标）
3. # 修改拷贝出来的配置文件, 再将配置文件拷贝回去容器 
    docker cp host_path containerID:container_path
4. # 再次启动容器
    docker start containerID


```

## 测试上传和访问

- 将图片上传到服务器/var/fdfs/storage/
- 进入storage-server

~~~shell
##进入容器
docker exec -it storage-server /bin/bash
##查看路径
ls /usr/bin/fdfs_upload_file
##上传
/usr/bin/fdfs_upload_file /etc/fdfs/client.conf /var/fdfs/test.jpg
##host对外访问8888
##然后访问File，浏览器访问（注意防火墙）
http://192.168.72.165:8888/group1/M00/00/00/wKhIo2PLNaeAfOgVAACnw_nFBnI646.jpg
# 记录返回的文件名称（卷名和文件名）信息
group1/M00/00/00/rBEAA2C2_mmAaiHtAAQNsWG90hY512.jpg
因为内置了一个Nginx，所以可以直接访问
~~~

## 开始搭建FastDFS集群

| IP地址         | 主机名称 | 备注            |
| -------------- | -------- | --------------- |
| 192.168.72.165 | node01   | tracker+storage |
| 192.168.72.164 | node01   | tracker+storage |

> 开放端口: 22122（tracker服务的端口）、23002（storage服务的端口）、8888（nginx服务的端口）
>
> 安装根目录：/data/fastdfs

### 拉取镜像包

```bash
docker pull morunchang/fastdfs
```

###  创建tracker工作目录

```bash
mkdir -p /data/fastdfs/tracker/data /data/fastdfs/tracker/conf
```

### 创建tracker.conf配置文件

```bash
cat <<EOF > /data/fastdfs/tracker/conf/tracker.conf
disabled=false
bind_addr=
port=22122
connect_timeout=30
network_timeout=30
base_path=/data/fast_data
max_connections=256
accept_threads=1
work_threads=4
store_lookup=2
store_group=group1
store_server=0
store_path=0
download_server=0
reserved_storage_space = 10%
log_level=info
run_by_group=
run_by_user=
allow_hosts=*
sync_log_buff_interval = 10
check_active_interval = 120
thread_stack_size = 64KB
storage_ip_changed_auto_adjust = true
storage_sync_file_max_delay = 86400
storage_sync_file_max_time = 300
use_trunk_file = false
slot_min_size = 256
slot_max_size = 16MB
trunk_file_size = 64MB
trunk_create_file_advance = false
trunk_create_file_time_base = 02:00
trunk_create_file_interval = 86400
trunk_create_file_space_threshold = 20G
trunk_init_check_occupying = false
trunk_init_reload_from_binlog = false
trunk_compress_binlog_min_interval = 0
use_storage_id = false
storage_ids_filename = storage_ids.conf
id_type_in_filename = ip
store_slave_file_use_link = false
rotate_error_log = false
error_log_rotate_time=00:00
rotate_error_log_size = 0
log_file_keep_days = 0
use_connection_pool = false
connection_pool_max_idle_time = 3600
http.server_port=8080
http.check_alive_interval=30
http.check_alive_type=tcp
http.check_alive_uri=/status.html
EOF
```

### 创建tracker.sh文件

```bash
cat <<EOF > /data/fastdfs/storage/conf/tracker.sh
#!/bin/sh
/data/fastdfs/tracker/fdfs_trackerd /etc/fdfs/tracker.conf
/etc/nginx/sbin/nginx
tail -f /data/fast_data/logs/trackerd.log
EOF
```

### 如果你开启了防火墙必须进行下面操作

```bash
# 放行22122端口
firewall-cmd --zone=public --add-port=22122/tcp --permanent
# 重新加载新的放行列表
firewall-cmd --reload
# 查看放行端口列表中是否存在
firewall-cmd --list-all
```

###  创建storage工作目录

```bash
mkdir -p /data/fastdfs/storage/data /data/fastdfs/storage/conf
```

### 创建storage.conf配置文件

```bash
cat <<EOF > /data/fastdfs/storage/conf/storage.conf
disabled=false
group_name=group1
bind_addr=
client_bind=true
port=23002
connect_timeout=30
network_timeout=30
heart_beat_interval=30
stat_report_interval=60
base_path=/data/fast_data
max_connections=256
buff_size = 256KB
accept_threads=1
work_threads=4
disk_rw_separated = true
disk_reader_threads = 1
disk_writer_threads = 1
sync_wait_msec=50
sync_interval=0
sync_start_time=00:00
sync_end_time=23:59
write_mark_file_freq=500
store_path_count=1
store_path0=/data/fast_data
subdir_count_per_path=256
# tracker集群（必须改为自己的IP）
tracker_server=192.168.72.165:22122
tracker_server=192.168.72.164:22122
log_level=debug
run_by_group=
run_by_user=
allow_hosts=*
file_distribute_path_mode=0
file_distribute_rotate_count=100
fsync_after_written_bytes=0
sync_log_buff_interval=10
sync_binlog_buff_interval=10
sync_stat_file_interval=300
thread_stack_size=512KB
upload_priority=10
if_alias_prefix=
check_file_duplicate=0
file_signature_method=hash
key_namespace=FastDFS
keep_alive=0
use_access_log = true
rotate_access_log = false
access_log_rotate_time=00:00
rotate_error_log = false
error_log_rotate_time=00:00
rotate_access_log_size = 0
rotate_error_log_size = 0
log_file_keep_days = 0
file_sync_skip_invalid_record=false
use_connection_pool = false
connection_pool_max_idle_time = 3600
http.domain_name=
http.server_port=8888
EOF
```

### 创建nginx.conf配置文件

```bash
cat <<EOF > /data/fastdfs/storage/conf/nginx.conf
user  root;
worker_processes  1;
error_log  /data/fast_data/logs/nginx-error.log;

events {
    worker_connections  1024;
}

http {
    include       mime.types;
    default_type  application/octet-stream;

    log_format  main  '$remote_addr - $remote_user [$time_local] "$request" '
                      '$status $body_bytes_sent "$http_referer" '
                      '"$http_user_agent" "$http_x_forwarded_for"';

    access_log  /data/fast_data/logs/nginx-access.log  main;
    sendfile        on;
    keepalive_timeout  65;

    server {
        listen       8888;
        server_name  localhost;

        location / {
            root   html;
            index  index.html index.htm;
        }

        location ~ /group1/M00 {
                    root /data/fast_data/data;
                    ngx_fastdfs_module;
        }

        error_page   500 502 503 504  /50x.html;
        location = /50x.html {
            root   html;
        }
    }
}
EOF
```

### 创建mod_fastdfs.conf配置文件

```bash
cat <<EOF > /data/fastdfs/storage/conf/mod_fastdfs.conf
connect_timeout=30
network_timeout=30
base_path=/data/fast_data
load_fdfs_parameters_from_tracker=true
storage_sync_file_max_delay = 86400
use_storage_id = false
storage_ids_filename = storage_ids.conf
#tracker集群
tracker_server=192.168.72.165:22122
tracker_server=192.168.72.164:22122
storage_server_port=23002
group_name=group1
url_have_group_name = true
store_path_count=1
store_path0=/data/fast_data
log_level=info
log_filename=
response_mode=proxy
if_alias_prefix=
flv_support = true
flv_extension = flv
group_count = 0

#HTTP default content type
http.default_content_type = application/octet-stream

#MIME types mapping filename
#MIME types file format: MIME_type extensions
#such as: image/jpeg jpeg jpg jpe
#you can use apache’s MIME file: mime.types
http.mime_types_filename=/etc/nginx/conf/mime.types
EOF
```

### 创建storage.sh启动脚本

```bash
cat <<EOF > /data/fastdfs/storage/conf/storage.sh
#!/bin/sh
/data/fastdfs/storage/fdfs_storaged /etc/fdfs/storage.conf
/etc/nginx/sbin/nginx
tail -f /data/fast_data/logs/storaged.log
EOF
```

### 创建client.conf配置文件

```bash
cat <<EOF > /data/fastdfs/storage/conf/client.conf
# connect timeout in seconds
# default value is 30s
connect_timeout=30

# network timeout in seconds
# default value is 30s
network_timeout=30

# the base path to store log files
base_path=/data/fast_data

# tracker_server can ocur more than once, and tracker_server format is
#  "host:port", host can be hostname or ip address
#tracker集群
tracker_server=192.168.72.165:22122
tracker_server=192.168.72.164:22122

#standard log level as syslog, case insensitive, value list:
### emerg for emergency
### alert
### crit for critical
### error
### warn for warning
### notice
### info
### debug
log_level=info

# if use connection pool
# default value is false
# since V4.05
use_connection_pool = false

# connections whose the idle time exceeds this time will be closed
# unit: second
# default value is 3600
# since V4.05
connection_pool_max_idle_time = 3600

# if load FastDFS parameters from tracker server
# since V4.05
# default value is false
load_fdfs_parameters_from_tracker=false

# if use storage ID instead of IP address
# same as tracker.conf
# valid only when load_fdfs_parameters_from_tracker is false
# default value is false
# since V4.05
use_storage_id = false

# specify storage ids filename, can use relative or absolute path
# same as tracker.conf
# valid only when load_fdfs_parameters_from_tracker is false
# since V4.05
storage_ids_filename = storage_ids.conf

#HTTP settings
http.tracker_server_port=80

#use "#include" directive to include HTTP other settiongs
#include http.conf
EOF
```

### 开放Storage的端口

```bash
firewall-cmd --zone=public --add-port=23002/tcp --permanent
firewall-cmd --zone=public --add-port=8888/tcp --permanent
firewall-cmd --reload
firewall-cmd --list-all
```

### 创建docker-compose.yml编排文件

```yml
cat <<EOF > /data/fastdfs/docker-compose.yml
version: '3.7'
services:
  fastdfs-tracker:
    image: morunchang/fastdfs
    container_name: fastdfs-tracker
    restart: always
    volumes:
      - /etc/localtime:/etc/localtime
      - /data/fastdfs/tracker/data:/data/fast_data
      - /data/fastdfs/tracker/conf/tracker.conf:/etc/fdfs/tracker.conf
    environment:
      - TZ=Asia/Shanghai
    network_mode: "host"
    command: "sh tracker.sh"

  fastdfs-storage:
    image: morunchang/fastdfs
    container_name: fastdfs-storage
    restart: always
    volumes:
      - /etc/localtime:/etc/localtime
      - /data/fastdfs/storage/data:/data/fast_data
      - /data/fastdfs/storage/conf/storage.sh:/storage.sh
      - /data/fastdfs/storage/conf/storage.conf:/etc/fdfs/storage.conf
      - /data/fastdfs/storage/conf/nginx.conf:/etc/nginx/conf/nginx.conf
      - /data/fastdfs/storage/conf/mod_fastdfs.conf:/etc/fdfs/mod_fastdfs.conf
      - /data/fastdfs/storage/conf/client.conf:/data/fastdfs/conf/client.conf
    environment:
      - TZ=Asia/Shanghai
    network_mode: "host"
    command: "sh storage.sh"
EOF
```

### 启动编排好的服务

```bash
cd /data/fastdfs/ && docker-compose up -d
```

### 查看启动日志

```bash
docker-compose logs -f
```

### 查看启动的服务

```bash
docker-compose ps
```

### 查看fastdfs集群运行情况

```bash
docker exec -it fastdfs-storage fdfs_monitor /data/fastdfs/conf/client.conf
```

> 参数说明 tracker_server_count：2 --表示2个Tracker Server tracker server is 192.168.163.130:22122 --表示Leader Tracker group count: 1 --表示有1个group group name = group1 --组名称是group1 storage server count = 2 --组内有2个storage active server count = 2 --活动的storage有2个 storage server port = 23002 --storage的端口 storage HTTP port = 8888   --storage的文件访问端口 store path count = 1 --storage只挂了一个存储目录 total_upload_count = 11 --总共上传了多少个文件 total_upload_bytes = 691405 --总共上传了多少字节 success_upload_bytes = 691405 --成功上传了多少字节 total_download_count = 2 --总共下载了多少文件（使用java客户端）