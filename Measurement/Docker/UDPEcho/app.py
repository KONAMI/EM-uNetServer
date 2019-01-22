import sys, struct, time, fcntl, sys, threading, datetime, subprocess, socket, errno, select, json, hashlib, io, binascii
from ctypes import *

"""
typedef struct {
    char mark[4]; // =>  0x52554450 = 'RUDP'
    uint16_t type;
    uint16_t len;
    uint32_t seq;
    char     *payload;
} RudpPacket
"""

"""    
headerSize = 12
msg = "{'x':10,'msg':'hogehoge'}".encode(encoding='utf-8')
msgLen = len(msg) + headerSize
msgType = 0x0001
msgSeq  = 0 
sendPkt = RudpPacket(b"RUDP", msgType, msgLen, msgSeq, msg)

buffer = io.BytesIO()
buffer.write(sendPkt)
buffer.seek(0)
DumpPacket(buffer.read(msgLen));
"""

# todo: self.checksum=hashlib.sha1(data).hexdigest()
# class RudpPacket(Structure):
class RudpPacket(BigEndianStructure):
    _fields_ = (
        ('mc', c_char * 4), # magic cookie
        ('type', c_uint16),
        ('len', c_uint16),
        ('seq', c_uint32),
        ('payload', c_char * 1280), # len - headesize = payload length
    )
    
class RudpServer():

    MSG_TYPE_DEFAULT = 0x0001
    MSG_TYPE_KEY     = 0x0002
    MSG_TYPE_ECHO    = 0x0003
    MSG_TYPE_ACK     = 0x0010    

    PKT_HEADER_SIZE  = 12
    
    def DumpPacket(self, pktBytes):
        buf = io.BytesIO(pktBytes)
        pkt = RudpPacket()
        buf.readinto(pkt)
        if pkt.mc != "RUDP":
            print "Invalid Packet."
            return
        msgLen = pkt.len
        buf.seek(0)
        hexDump = binascii.hexlify(buf.read(msgLen))
        
        seek = 0
        unit = 8
        hexLen = len(hexDump)
        print "--[ packet dump ]-------------------------------------------"
        while seek < hexLen:
            seekEnd = seek + unit    
            if seekEnd > hexLen:
                seekEnd = hexLen
            print hexDump[seek:seekEnd]
            seek += unit
        print "------------------------------------------------------------"

    def CreateListener(self, port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind(("", port))
        self.sock.setblocking(0)
	self.socklist = []
        self.socklist.append(self.sock)        

    def DestroyListener(self):
        self.sock.close()

    def AppPacketRecvHandler(self, pkt, peer):
        buf = io.BytesIO()
        if pkt.type == self.MSG_TYPE_ECHO:
            pkt.type = self.MSG_TYPE_ACK # ack
            buf.write(pkt)
            buf.seek(0)
            self.sock.sendto(buf.read(pkt.len), peer)
            self.pps_s += 1
        else :
            if pkt.type == self.MSG_TYPE_KEY:
                pkt.type = self.MSG_TYPE_ACK # ack
                pkt.len  = self.PKT_HEADER_SIZE
                buf.write(pkt)
                buf.seek(0)
                self.sock.sendto(buf.read(self.PKT_HEADER_SIZE), peer)        
                self.pps_s += 1
            # @todo : Application Process
        
    def UpdateListener(self):
        read_sockets, write_sockets, error_sockets = select.select(self.socklist, [], [], 0)
        for s in read_sockets:
	    if s == self.sock:
                try:
                    rBuf, peer = self.sock.recvfrom(self.rBufSize)
                    self.pps_r += 1
                    #self.DumpPacket(rBuf)
                    buf = io.BytesIO(rBuf)
                    pkt = RudpPacket()
                    buf.readinto(pkt)
                    if pkt.mc != "RUDP":
                        print "Invalid MC > Ignore."
                    else:
                        self.AppPacketRecvHandler(pkt, peer)
                            
                except socket.error, v:
		    errorcode = v[0]
		    print("socket recv error. >> " + errorcode)
	        return

    def Start(self, listenPort):
        self.rBufSize = 4096
        self.CreateListener(listenPort)
        self.pps_r = 0
        self.pps_s = 0

    def Stop(self):
        self.DestroyListener()

    def Update(self):
        self.UpdateListener();

if __name__ == '__main__':
    pRudpServer = RudpServer()
    pRudpServer.Start(5730)
    tick = 1
    now  = time.time()
    
    while True:
        
        pRudpServer.Update()
        
        current = time.time()
        waitBy  = now + (1 / 60.0 * tick)
        
        if waitBy > current:
            time.sleep(waitBy - current)
            current = time.time()
            
        if current - now >= 1.0:
            print "fps : " + str(tick) + " pps " + str(pRudpServer.pps_r) + "/" + str(pRudpServer.pps_s)  
            now += 1.0
            tick = 0
            pRudpServer.pps_r = 0
            pRudpServer.pps_s = 0
            
        tick += 1
    pRudpServer.Stop()
        
