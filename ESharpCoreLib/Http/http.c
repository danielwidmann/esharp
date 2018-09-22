#include <stdio.h>
#include <string.h>
#include <stdlib.h>
//#include "Winsock2.h"

//void PCSimpleHttpClient_ctor(PCSimpleHttpClient _this, string BaseAddress)
//{
//
//}
char* get(char* server, char* url);

//string PCSimpleHttpClient_Get(PCSimpleHttpClient _this, string path)
//{
//	return get(_this->_base.BaseAddress_k__BackingField, path);
//}
//
//string SimpleHttpClient_Get_novirtual(SimpleHttpClient _this, string path)
//{
//	return "";
//	//return PCSimpleHttpClient_Get(_this, path);
//}


typedef struct {
	//PCSimpleHttpClient _this;
	string host;
	string path;
	Action_String callback;
} getArgs;

void callGet(getArgs* arg)
{
	string res = get(arg->host, arg->path);

	ScheduleHelper_ScheduleActionString(arg->callback, res);

	EObject_RemoveRef((EObject)arg->callback);
	free(arg);
}

void HttpClient_Request(string server, string path, Action_String callback)

{
	getArgs* arg = (getArgs*)malloc(sizeof(getArgs));

	EObject_AddRef((EObject)callback);

	arg->callback = callback;
	arg->path = path;
	arg->host = server;

	if (NULL == CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)&callGet, arg, 0, NULL))
	{
		printf("error creating thread");
	}
}

//void PCSimpleHttpClient_GetAsync(PCSimpleHttpClient _this, string path, StringCallback callback)
//{
//	getArgs* arg = (getArgs*)malloc(sizeof(getArgs));
//
//	EObject_AddRef((EObject)callback);
//
//	arg->callback = callback;
//	arg->path = path;
//	arg->_this = _this;
//
//	if (NULL == CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)&callGet, arg, 0, NULL))
//	{
//		printf("error creating thread");
//	}
//
//}

#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdlib.h>
#include <stdio.h>
#include <windns.h>   //DNS api's

// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")
#pragma comment (lib, "dnsapi.lib")


#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "80"
char recvbuf[DEFAULT_BUFLEN];

char* get(char* hostName, char* url)
{
	char* ret;
	WSADATA wsaData;
	SOCKET ConnectSocket = INVALID_SOCKET;
	struct addrinfo *result = NULL,
		*ptr = NULL;

	struct addrinfo hints;
	char sendbuf[DEFAULT_BUFLEN];
	sprintf_s(sendbuf, sizeof(sendbuf), "GET %s HTTP/1.1\r\nHost: %s\r\n\r\n", url, hostName);
	//sprintf_s(sendbuf, sizeof(sendbuf), "GET /input/OGqm0JVvGquK1ww558xR?private_key=8bVZxawlbVS5RmmrrNwv&temp=11.40 HTTP/1.1\r\nHost: data.sparkfun.com\r\n\r\n");


	PDNS_RECORD res;
	IN_ADDR ipaddr;

	DNS_STATUS dnsRes = DnsQuery_A(hostName, DNS_TYPE_A, DNS_QUERY_STANDARD, NULL, &res, NULL);

	if (dnsRes != 0)
	{
		printf("Dns Error");
		return CharPtr_To_String("");
	}

	//convert the Internet network address into a string
	//in Internet standard dotted format.
	ipaddr.S_un.S_addr = (res->Data.A.IpAddress);
	//printf("The IP address of the host %s is %s \n", hostName, inet_ntoa(ipaddr));

	// Free memory allocated for DNS records. 
	DnsRecordListFree(res);

	WSADATA wsa;
	SOCKET s;
	struct sockaddr_in server;
	char *message, server_reply[4000];
	int recv_size;

	printf("\nInitialising Winsock...");
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("Failed. Error Code : %d", WSAGetLastError());
		return CharPtr_To_String("");
	}

	printf("Initialised.\n");

	//Create a socket
	if ((s = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET)
	{
		printf("Could not create socket : %d", WSAGetLastError());
	}

	printf("Socket created.\n");


	//server.sin_addr.s_addr = inet_addr("54.86.132.254");
	server.sin_addr = ipaddr;
	server.sin_family = AF_INET;
	server.sin_port = htons(80);

	//Connect to remote server
	if (connect(s, (struct sockaddr *)&server, sizeof(server)) < 0)
	{
		puts("connect error");
		return CharPtr_To_String("");
	}

	puts("Connected");

	//Send some data
	//message = "GET / input / OGqm0JVvGquK1ww558xR ? private_key = 8bVZxawlbVS5RmmrrNwv & temp = 11.40 HTTP / 1.1\r\nHost: data.sparkfun.com\r\n\r\n";
	if (send(s, sendbuf, strlen(sendbuf), 0) < 0)
	{
		puts("Send failed");
		return CharPtr_To_String("");
	}
	puts("Data Send\n");

	//Receive a reply from the server
	if ((recv_size = recv(s, server_reply, 2000, 0)) == SOCKET_ERROR)
	{
		puts("recv failed");
	}

	puts("Reply received\n");

	//Add a NULL terminating character to make it a proper string before printing
	server_reply[recv_size] = '\0';
	puts(server_reply);

	return CharPtr_To_String("");








	int iResult;
	int recvbuflen = DEFAULT_BUFLEN;



	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != 0) {
		printf("WSAStartup failed with error: %d\n", iResult);
		return CharPtr_To_String("");
	}

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	// Resolve the server address and port
	iResult = getaddrinfo(hostName, DEFAULT_PORT, &hints, &result);
	if (iResult != 0) {
		printf("getaddrinfo failed with error: %d\n", iResult);
		WSACleanup();
		return CharPtr_To_String("");
	}

	// Attempt to connect to an address until one succeeds
	for (ptr = result; ptr != NULL; ptr = ptr->ai_next) {

		// Create a SOCKET for connecting to server
		ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype,
			ptr->ai_protocol);
		if (ConnectSocket == INVALID_SOCKET) {
			printf("socket failed with error: %ld\n", WSAGetLastError());
			WSACleanup();
			return CharPtr_To_String("");
		}

		// Connect to server.
		iResult = connect(ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
		if (iResult == SOCKET_ERROR) {
			closesocket(ConnectSocket);
			ConnectSocket = INVALID_SOCKET;
			continue;
		}
		break;
	}

	freeaddrinfo(result);

	if (ConnectSocket == INVALID_SOCKET) {
		printf("Unable to connect to server!\n");
		WSACleanup();
		return CharPtr_To_String("");
	}

	// Send an initial buffer
	iResult = send(ConnectSocket, sendbuf, (int)strlen(sendbuf), 0);
	if (iResult == SOCKET_ERROR) {
		printf("send failed with error: %d\n", WSAGetLastError());
		closesocket(ConnectSocket);
		WSACleanup();
		return CharPtr_To_String("");
	}

	printf("Bytes Sent: %ld\n", iResult);

	// shutdown the connection since no more data will be sent
	iResult = shutdown(ConnectSocket, SD_SEND);
	if (iResult == SOCKET_ERROR) {
		printf("shutdown failed with error: %d\n", WSAGetLastError());
		closesocket(ConnectSocket);
		WSACleanup();
		return CharPtr_To_String("");
	}

	// Receive until the peer closes the connection
	do {

		iResult = recv(ConnectSocket, recvbuf, recvbuflen, 0);
		if (iResult > 0)
			printf("Bytes received: %d\n", iResult);
		else if (iResult == 0)
			printf("Connection closed\n");
		else
			printf("recv failed with error: %d\n", WSAGetLastError());

	} while (iResult > 0);

	// cleanup
	closesocket(ConnectSocket);
	WSACleanup();

	printf(recvbuf);

	return CharPtr_To_String(recvbuf);
}