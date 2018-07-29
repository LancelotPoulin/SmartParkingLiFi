#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sstream>

#include <wiringPi.h>
#include <cstdio>
#include <ctime>
#include <time.h>

#include <cppconn/driver.h>
#include <cppconn/exception.h>
#include <cppconn/resultset.h>
#include <cppconn/statement.h>

#include <unistd.h>			//Used for UART
#include <fcntl.h>			//Used for UART
#include <termios.h>		//Used for UART

#define LIGHT_DETECTION_CONST 5000
#define RED_LED_PIN 4
#define GREEN_LED_PIN 5
#define LDR_PIN 7

using namespace std;

void GenerateReservationDateTimes(string &BeginDate, string &EndDate);
string GenerateUniqueReference();
bool ConnectToDatabase();
string ReadFlashData();
void SendLiFiData(string Data);
bool IsPlaceAvailable(string BeginDate, string EndDate);

int UartFileStream = -1;
sql::Driver* driver;
sql::Connection* con;
sql::Statement* stmt;
