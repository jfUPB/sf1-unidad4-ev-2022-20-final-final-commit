# Evaluación unidad #4 
## Equipo: *FINAL FINAL COMMIT*

### Participantes: 

* William Alejandro Pabón - ID: 225366
* Juan Pablo Correa - ID: 449689
* Manuela Cuervo - ID: 446641

## Enunciado

"Acabas de llegar a un nuevo estudio que desarrolla EXPERIENCIAS INTERACTIVAS y te encargan que DISEÑES e IMPLEMENTES una EXPERIENCIA INTERACTIVA que use un acelerómetro (al menos dos ejes) para interactuar con ella.

Tienes las siguientes restricciones:

1. El proyecto debe hacerse en Unity.

2. La aplicación debe utilizar un plugin llamado Ardity para realizar la integración.

![MPU6050 and ESP32](https://content.instructables.com/ORIG/F1U/RBUZ/KFDYWZIZ/F1URBUZKFDYWZIZ.png?auto=webp&frame=1&fit=bounds&md=773d9ab9ee3bf6f0f90e33771377beb5)


* Entregar: El código fuente de las aplicaciones para el microcontrolador y para Unity en este repositorio y la documentación de cómo integraste la información del sensor para modificar el comportamiento de la aplicación. Esta documentación la debes incluir en el archivo README.md.

### Referencias

* Ardity
> https://docs.google.com/presentation/d/1uHoIzJGHLZxLbkMdF1o_Ov14xSD3wP31-MQtsbOSa2E/edit#slide=id.p
>https://assetstore.unity.com/packages/tools/integration/ardity-arduino-unity-communication-made-easy-123819

* MPU-6050 Six-Axis (Gyro + Accelerometer) unity 3d and Arduino
> https://www.youtube.com/watch?v=L4WfHT_58Dg

#
## Review del código (Microcontroller)


* Se hace la carga de las librearias de Adafruit para poder comunicarse con el sensor mpu6050 y se decalaran las variables del sensor y los desplazamientos.

* En el void set up se establece la velocidad de la conexion, se corre tambien un chequeo de conexion con el sensor y se establecen los valores y rangos 
de medicion del sensor

```cpp
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

```

* Luego en el void loop se llama los eventos de los cuales se obtendran las lecturas del sensor y luego se usa trigonometria para calcular el movimiento en el eje horizontal y vertical del microcontrolador.

*Tambien se establece la conexion con Unity con el command que en este caso sera com, y que cada ves que se llame, segun la rotacion del sensor mandara la letra respectiva del input.

```cpp
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

```

#
## Review del código (UNITY)


* Aqui en Unity el trabajo de comunicaciones se hace con las variables del header Coms, en este caso el atributo SerialController, un stringComparer, varibles de
espera en los float time y waitTime; y la varibale de message que sera lo que se lee del micro y un catcher que la va a manejar.

```cpp
[Header("Serial Coms")]
    public SerialController serialController;
    StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
    
    private float waitTime = 0.060f;
    private float time = 0.0f;
    
    public string message;
    string catcher;

```

* La aplicacion de Unity  realiza la comunicacion llamando a los metodos del serial controller y enviado el 
llamado con un wait de 60 milisegundos, luego recibira la informacion en el metodo read, de aca se comprueba
el contenido de message, luego esto se pasa al catcher y se tienen 3 casos stringComparer que acceden al atributo
velocity del rigidBody que modifican el movimiento.

```cpp
 //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        time += Time.deltaTime;

        if (time > waitTime)
        {
            time = time - waitTime;
            Debug.Log("Sending com");
            serialController.SendSerialMessage("com\n");
        }
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------
        message = serialController.ReadSerialMessage();
        Debug.Log(message);

        if(message != null)
        {
            // Debug.Log(message);
            catcher = message;
            if (stringComparer.Equals("A", catcher))
            {
                rb.velocity = Vector2.left * 10;
                Debug.Log("[A]");
            }
            if (stringComparer.Equals("D", catcher))
            {
                rb.velocity = Vector2.right * 10;
                Debug.Log("[D]");
            }
            if (stringComparer.Equals("W", catcher))
            {
                rb.velocity = Vector2.up * 2;
                Debug.Log("[W]");
            }
        }
```
