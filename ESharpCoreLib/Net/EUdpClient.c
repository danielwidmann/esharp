#ifdef _WIN32
/* See http://stackoverflow.com/questions/12765743/getaddrinfo-on-win32 */
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0501  /* Windows XP. */
#endif
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <stdio.h>

#pragma comment(lib, "Ws2_32.lib")

#else
/* Assume that any non-Windows platform uses POSIX-style sockets instead. */
#include <sys/socket.h>
#include <arpa/inet.h>
#include <netdb.h>  /* Needed for getaddrinfo() and freeaddrinfo() */
#include <unistd.h> /* Needed for close() */
#endif

int sockInit(void)
{
#ifdef _WIN32
	WSADATA wsa_data;
	return WSAStartup(MAKEWORD(1, 1), &wsa_data);
#else
	return 0;
#endif
}

int sockQuit(void)
{
#ifdef _WIN32
	return WSACleanup();
#else
	return 0;
#endif
}

/* Note: For POSIX, typedef SOCKET as an int. */

int sockClose(SOCKET sock)
{

	int status = 0;

#ifdef _WIN32
	status = shutdown(sock, SD_BOTH);
	if (status == 0) { status = closesocket(sock); }
#else
	status = shutdown(sock, SHUT_RDWR);
	if (status == 0) { status = close(sock); }
#endif

	return status;

}

void EUdpClient_ctor(EUdpClient _this) {


}

EUdpClient EUdpClient_new() {
	EUdpClient r = CMalloc_Malloc(EUdpClient_TypeId);
	r->_base.etype = EUdpClient_TypeId;
	r->_base.refCount = 1;

	return r;
}

void EUdpClient_Finalize(EUdpClient _this) {


}

void EUdpClient_Connect(EUdpClient _this, string destination, int32_t port) {
	
	sockInit();
	int fd;


	if ((fd = socket(AF_INET, SOCK_DGRAM, 0)) < 0) {
		perror("cannot create socket");
		return 0;
	}
	struct sockaddr_in myaddr;

	/* bind to an arbitrary return address */
	/* because this is the client side, we don't care about the address */
	/* since no application will initiate communication here - it will */
	/* just send responses */
	/* INADDR_ANY is the IP address and 0 is the socket */
	/* htonl converts a long integer (e.g. address) to a network representation */
	/* htons converts a short integer (e.g. port) to a network representation */

	memset((char *)&myaddr, 0, sizeof(myaddr));
	myaddr.sin_family = AF_INET;
	myaddr.sin_addr.s_addr = htonl(INADDR_ANY);
	myaddr.sin_port = htons(0);

	if (bind(fd, (struct sockaddr *)&myaddr, sizeof(myaddr)) < 0) {
		perror("bind failed");
		return 0;
	}

	_this->socket = fd;
}
ETask_obj EUdpClient_SendAsync(EUdpClient _this, Array_1 data, int32_t bytes)
{
	struct sockaddr_in RecvAddr;

	RecvAddr.sin_family = AF_INET;
	RecvAddr.sin_port = htons(53);
	//RecvAddr.sin_addr.s_addr
	RecvAddr.sin_addr.s_addr = 8| 8 << 8 | 8 << 16 | 8 << 24 ;
	//RecvAddr.sin_addr.s_addr = inet_addr("8.8.8.8");
	//inet_pton(AF_INET, "8.8.8.8", &(RecvAddr.sin_addr));

	sendto(_this->socket, data->data, data->Length, 0, &RecvAddr, sizeof(RecvAddr));
	ETask_obj task = (ETask)CMalloc_Malloc(ETask_TypeId);
	ETask_obj_ctor(task);
	//BoxedInt32 value;// = BoxedInt32_new();
	//value.value = 0;
	EObject res = Boxed_Int32_box(0);
	ETask_obj_SetResult(task, res);
	EObject_RemoveRef(res);
	return task;
}

void receive_waiter(EUdpClient _this) {
	uint8_t data[1000];
	int size = recv(_this->socket, data, sizeof(data),0);
	if (size < 0) {
		printf("error receive %d\n", size);
		return;
	}

	Array_1 bytes = CMalloc_ArrayMalloc_1(size);
	memcpy(bytes->data, data, size);
	EUdpReceiveResult res = EUdpReceiveResult_FormBytes(bytes);
	EObject box = Boxed_UdpReceiveResult_box(res);
	ETask_obj_SetResult(_this->m_receiveResult, box);
	EObject_RemoveRef(box);
	EUdpReceiveResult_RemoveRef(&res);
	EObject_RemoveRef(bytes);
	EObject_RemoveRef(_this->m_receiveResult);
}

ETask_obj EUdpClient_ReceiveAsync(EUdpClient _this)
{
	ETask_obj task = (ETask_obj)CMalloc_Malloc(ETask_obj_TypeId);
	ETask_obj_ctor(task);
	_this->m_receiveResult = task;
	EObject_AddRef(task);

	task->m_syncContext = ESyncronizationContext_get_Current();

	if (NULL == CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)&receive_waiter, _this, 0, NULL))
	{
		printf("error creating thread");
	}

	return task;
}
