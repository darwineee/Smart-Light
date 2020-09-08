#include <Wire.h>
byte buf[8];
uint16_t relay1[4][2] = {{0,0},{0,0},{0,0},{0,0}};
uint16_t relay2[4][2] = {{0,0},{0,0},{0,0},{0,0}};
uint16_t relay3[4][2] = {{0,0},{0,0},{0,0},{0,0}};
uint16_t relay4[4][2] = {{0,0},{0,0},{0,0},{0,0}};
void setup(){
  Wire.begin(8);
  Wire.onReceive(receiveEvent);
  Serial.begin(9600);
}
void loop(){
  delay(100);
}
void receiveEvent(int howMany) {
  byte flag = 0;
  while (0 < Wire.available()) {
    char dAta = Wire.read();      /* receive byte as a character */
    if(isdigit(dAta)){ 
        buf[flag] = (byte)dAta-48;   
        flag++;
    }
  }
  analyse_buf(buf[0],buf[1],buf[2]*1000 + buf[3]*100 + buf[4]*10 + buf[5]);
}

void analyse_buf(uint8_t sw_cmd, uint8_t dia_chi, uint16_t du_lieu){
  switch(sw_cmd){
    case 1:
      relay1[dia_chi/2][dia_chi%2] = du_lieu;
      //Serial.println(du_lieu);
      break;
    case 2:
      relay2[dia_chi/2][dia_chi%2] = du_lieu;
      break;
    case 3:
      relay3[dia_chi/2][dia_chi%2] = du_lieu;
      break;
    case 4:
      relay4[dia_chi/2][dia_chi%2] = du_lieu;
      break;
  }
}
