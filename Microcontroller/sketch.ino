#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <Arduino.h>

Adafruit_MPU6050 mpu;
float roll, pitch,yaw;

void setup(void) {
  Serial.begin(115200);
  while (!Serial)
    delay(10);
  //Serial.println("Adafruit MPU6050 test!");
  // Try to initialize!
  if (!mpu.begin()) {
    //Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }

  //Serial.println("MPU6050 Found!");
  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  //Serial.print("Accelerometer range set to: RANGE_8_G");
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  //Serial.print("Gyro range set to: RANGE_500_DEG");
  mpu.setFilterBandwidth(MPU6050_BAND_5_HZ);
  //Serial.print("Filter bandwidth set to: BAND_5_HZ");
}

void loop() {
  /* Get new sensor events with the readings */
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  roll = 180 *atan(a.acceleration.x/sqrt(a.acceleration.y*a.acceleration.y+a.acceleration.z*a.acceleration.z))/M_PI;
  pitch = 180 *atan(a.acceleration.y/sqrt(a.acceleration.x*a.acceleration.x+a.acceleration.z*a.acceleration.z))/M_PI;

  if(Serial.available() > 0)
  {
    String command = Serial.readStringUntil('\n');

    if(command == "com")
    {
      if(roll < -20)
        Serial.println("D");
      if(roll > 20)
        Serial.println("A");
      if(pitch > 20)
        Serial.println("W");
    }        
  }    
}