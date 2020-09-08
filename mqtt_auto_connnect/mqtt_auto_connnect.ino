#include <PubSubClient.h>
#include <ESP8266WiFi.h>         
#include <DNSServer.h>
#include <ESP8266WebServer.h>
#include <Wire.h>
#include "WiFiManager.h"  


void nhan_dl(char* tp, byte* nd,unsigned int do_dai){
  String topic(tp);
  String noidung = String((char*)nd);
  noidung.remove(do_dai);
  Wire.beginTransmission(8);
  Wire.write((topic+noidung).c_str());
  Wire.endTransmission();
}

WiFiClient esp_client;
PubSubClient MQTT("mqtt.ngoinhaiot.com",1111,nhan_dl,esp_client);       
void setup()
{
  Serial.begin(9600);
  WiFiManager wifiManager;   //Khai báo wifiManager thuộc class WiFiManager, được quy định trong file WiFiManager.h
  if (!wifiManager.autoConnect("Connect to setting","11111111"))
  {
    ESP.reset();
    delay(1000);
  }
  MQTT.connect("ESP","tung_thanh_nguyen","aJVISmwF");
  MQTT.subscribe("tung_thanh_nguyen/1/#");
  MQTT.subscribe("tung_thanh_nguyen/2/#");
  MQTT.subscribe("tung_thanh_nguyen/3/#");
  MQTT.subscribe("tung_thanh_nguyen/4/#");
  Wire.begin(D1,D2);
}

void loop()
{
  MQTT.loop();


}
