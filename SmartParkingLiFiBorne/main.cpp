#include "main.h"

// Convertit un type nombre en string
template <typename T>
string NumberToString(T Number)
{
    ostringstream ss;
    ss << Number;
    return ss.str();
}



// Point d'entrée de l'application
int main()
{
    // Setup WiringPi
    if(wiringPiSetup() == -1)
    {
        printf("Erreur initialisation wiringPi");
        return 1;
    }

    pinMode(RED_LED_PIN, OUTPUT);
    pinMode(GREEN_LED_PIN, OUTPUT);
    digitalWrite(RED_LED_PIN, HIGH);
    digitalWrite(GREEN_LED_PIN, LOW);

    // Setup WiringPi
    if(!ConnectToDatabase())
    {
        printf("Erreur connection BDD");
        return 1;
    }


    //OPEN THE UART
	UartFileStream = open("/dev/ttyS0", O_RDWR | O_NOCTTY | O_NDELAY); //Open in non blocking read/write mode
	if (UartFileStream == -1)
	{
		printf("Erreur initialisation UART\n");
		return 1;
    }

	//CONFIGURE THE UART
	struct termios options;
	tcgetattr(UartFileStream, &options);
	options.c_cflag = B115200 | CS8 | CLOCAL | CREAD; // Set baud rate
	options.c_iflag = IGNPAR;
	options.c_oflag = 0;
	options.c_lflag = 0;
	tcflush(UartFileStream, TCIFLUSH);
	tcsetattr(UartFileStream, TCSANOW, &options);


    while (true) // Main loop
    {
        cout << "En attente d'un client..." << endl;
        string Reference = ReadFlashData(); // Attente d'un client
        cout << "Reference: " << Reference << endl;
        if (atoi(Reference.c_str()) >= 1000)
        {
            string Place = ""; // Récupère une place disponible
            string PlaceID = "";
            sql::ResultSet* res1 = stmt->executeQuery("SELECT e.Numero, a.Lettre, p.Numero, p.ID FROM place p, allee a, etage e WHERE p.Disponible = 1 AND p.allee_id = a.id AND a.etage_id = e.id");
            while (res1->next())
            {
                Place = res1->getString(1) + "|" + res1->getString(2) + "|" + res1->getString(3);
                PlaceID = res1->getString(4);
            }
            delete res1;
            if (Place != "") // Si il y en a une
            {
                if (Reference == "1000") // Invite
                {
                    Reference = GenerateUniqueReference();
                    string BeginDate = ""; string EndDate = "";
                    GenerateReservationDateTimes(BeginDate, EndDate);
                    if (!IsPlaceAvailable(BeginDate, EndDate)) // Si parking non complet avec l'arrivée d'adhérents ayant réservés
                    {
                        cout << "Erreur: Pas de place disponible en invité \n" << endl;
                    }
                    else // Insertion réservation invité
                    {
                        stmt->execute("INSERT INTO reservation VALUES (NULL, " + Reference + ", '" + BeginDate + "', '" + EndDate + "', NULL, NULL, " + PlaceID + ", 1)");
                        SendLiFiData(Reference + "|" + Place); // Envoi référence + place
                        digitalWrite(RED_LED_PIN, LOW); // Allume la LED verte de passage
                        digitalWrite(GREEN_LED_PIN, HIGH);
                        delay(5000);
                        digitalWrite(RED_LED_PIN, HIGH);
                        digitalWrite(GREEN_LED_PIN, LOW);
                    }
                }
                else // Adhérent
                {
                    sql::ResultSet* res = stmt->executeQuery("SELECT Reference, Place_ID FROM reservation WHERE Reference = " + Reference); // Récupère laréservation adhérent
                    bool ExistAndUnused = false;
                    while (res->next())
                    {
                        if (res->getString(2) == "NULL") { ExistAndUnused = false; } else { ExistAndUnused = true; }
                    }
                    if (ExistAndUnused) // Si référence existe bien
                    {
                        stmt->execute("UPDATE reservation SET Place_ID = " + PlaceID + " WHERE Reference = " + Reference); // Met a jour la réservation adhérent avec la place attribué
                        SendLiFiData(Reference + "|" + Place); // envoi référence + place
                        digitalWrite(RED_LED_PIN, LOW); // Allume la LED verte de passage
                        digitalWrite(GREEN_LED_PIN, HIGH);
                        delay(5000);
                        digitalWrite(RED_LED_PIN, HIGH);
                        digitalWrite(GREEN_LED_PIN, LOW);
                    }
                    else
                    {
                        cout << "Erreur: Reference inexistante ou réservation déja utilisée \n" << endl;
                        delay(1000);
                    }
                }
            }
            else
            {
                cout << "Erreur: Pas de place disponible \n" << endl;
                delay(1000);
            }
        }
        else
        {
            cout << "Erreur: Référence impossible \n" << endl;
            delay(1000);
        }
    }

    // CLOSE THE UART and BDD connection
	close(UartFileStream);
	con->close();

    return 0;
}



// Récupère le nombre de place disponible entre l'intervalle de temps indiqué (Refactorisation)
bool IsPlaceAvailable(string BeginDate, string EndDate)
{
    sql::ResultSet* res2 = stmt->executeQuery("SELECT COUNT(*) FROM reservation WHERE Debut_Reservation < '" + EndDate + "' AND Fin_Reservation > '" + BeginDate + "'");
    string NbPlaceDispo = "0";
    while (res2->next())
    {
        NbPlaceDispo = res2->getString(1);
    }
    delete res2;
    sql::ResultSet* res3 = stmt->executeQuery("SELECT COUNT(*) FROM place");
    string NbPlaceTotal = "0";
    while (res3->next())
    {
        NbPlaceTotal = res3->getString(1);
    }
    delete res3;
    return (atoi(NbPlaceTotal.c_str()) - atoi(NbPlaceDispo.c_str()) != 0);
}



// Génère les dates de réservation invité lors de la création (Date actuelle jusqu'a Date actuelle + 2 heures) (Refactorisation)
void GenerateReservationDateTimes(string &BeginDate, string &EndDate)
{
    time_t current; struct tm datetime; char format[32];
    time(&current); datetime = *localtime(&current); strftime(format, 32, "%Y-%m-%d %H:%M:%S", &datetime);
    string Convert1(format); BeginDate = Convert1;
    current += 60 * 60 * 2; datetime = *localtime(&current); strftime(format, 32, "%Y-%m-%d %H:%M:%S", &datetime); // DateTime + 2 heures
    string Convert2(format); EndDate = Convert2;
}



// Génère une référence unique à partir de la BDD (Refactorisation)
string GenerateUniqueReference()
{
    string Reference = "";
    bool Exist = true;
    while (Exist) // Tant qu'elle existe on loop
    {
        Reference = NumberToString(rand() % (9999 - 1001) + 1001);
        sql::ResultSet* res = stmt->executeQuery("SELECT Reference FROM reservation WHERE Reference = " + Reference);
        Exist = false;
        while (res->next())
        {
            Exist = true; // Si existe déja alors loop
        }
    }
    return Reference;
}



// Envoi de données Li-Fi par les lampes à LED en communication UART
void SendLiFiData(string Data)
{
    const char* Buffer = Data.c_str();
    cout << "Envoi LiFi en cours: " << Data << "\n";
    int Tries = 150; // On envoie 150 fois à intervalle de 100ms les données
    while(Tries != 0)
    {
        int count = write(UartFileStream, Buffer, strlen(Buffer)); //Filestream, bytes to write, number of bytes to write
        if (count < 0)
        {
            printf("Erreur écriture UART\n");
            return;
        }
        delay(100);
        Tries--;
    }
    cout << "Fin Envoi LiFi\n\n";
}



// Connection à la BDD
bool ConnectToDatabase()
{
    try
    {
        // Etape 1 : créer une connexion à la BDD
        driver = get_driver_instance();
        con = driver->connect("mysql-projetlifi.alwaysdata.net", "156739", "projetlifi");

        // Etape 2 : connexion à la base choisie, ici olivier_db
        con->setSchema("projetlifi_bdd");
        stmt = con->createStatement();
        return true;
    }
    catch (sql::SQLException &e)
    {
        // Gestion des execeptions pour déboggage
        cout << "# ERR: " << e.what();
        cout << " (code erreur MySQL: " << e.getErrorCode();
        cout << ", EtatSQL: " << e.getSQLState() << " )" << endl;
    }
    return false;
}



// Lit la référence recu par la LDR
// Plus le temps de décharge du condensateur est court, plus la résistance de la LDR est petite, et donc plus il y a de la lumière
// Forte lumière = flash du téléphone, on lance un autre chrono pour savoir combien dur le flash (100ms pour 1, 200ms pour 2, ...)
// Le condensateur joue la rôle de "bloqueur" de tension
string ReadFlashData()
{
    string Data = "";
    clock_t LDRClock = clock();
    clock_t LightClock = clock();
    bool IsLightDetected = false;

    while (Data.length() < 4) // Référence a 4 chiffres =  on attend 4 valeurs
    {
        pinMode(LDR_PIN, OUTPUT); // Mise a l'état LOW
        digitalWrite(LDR_PIN, LOW);
        delayMicroseconds(80);

        pinMode(LDR_PIN, INPUT); // On remet la PIN en entrée
        delayMicroseconds(20);

        LDRClock = clock();
        while (digitalRead(LDR_PIN) == LOW) { } // On attend que la PIN soit HIGH = condensateur déchargé
        double LDRDuration = (clock() - LDRClock); // Temps de décharge
        // printf("%f \n", LDRDuration); // Pour voir les temps de décharge

        if (LDRDuration < LIGHT_DETECTION_CONST) // Si flash détécté, on lance le chrono ou on le continue
        {
            if (!IsLightDetected)
            {
                LightClock = clock();
                IsLightDetected = true;
            }
        }
        else // Si plus de flash détécté, on calcul la valeur avec la durée du chrono
        {
            if (IsLightDetected)
            {
                int LightDuration =(int)(((clock() - LightClock) / (double) CLOCKS_PER_SEC) * 10);
                if (LightDuration == 10) { LightDuration = 0; }
                if (!(Data.length() == 0 && LightDuration == 0))
                {
                    Data += NumberToString(LightDuration);
                }
                IsLightDetected = false;
            }
        }
    }
    return Data;
}
