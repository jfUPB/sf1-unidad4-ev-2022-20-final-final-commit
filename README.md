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


* Aquí hacemos lo más escencial que es inicializar las librerías y dependencias necesarias para poder hacer la comunicación del programa y el micro-controlador posible. Después procedemos a referenciar y declarar las variables que usaremos en nuestro código.

* Después de esto se inicializa la función *Void setup* la cual se encarga de ejecutarse cada vez que inicia el programa, así buscando el puerto serial correspondiente e iniciandolo y buscando la cominicación con el giroscopio.

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

* Posteriormente está la clase *Void Loop*, la cual está encargada de ejecutarse cada frame para poder 
tener cierto flujo de datos que nos envían unas ecuaciones hechas para calcular el movimiento de los 
ejes del acelerómetro y estos se les asignas a unas variables que usaremos más tarde. Lo que ejecuta 
después de esto nos permite verificar si hay algún puerto serial con datos disponibles, por lo que después 
revisa si ese dato es igual a *S* debido a que escogimos este mensaje para comunicar el Micro-Controlador con la aplicación *UNITY*.

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


* Aquí hacemos lo más escencial que es inicializar las librerías y dependencias necesarias para poder hacer la comunicación del programa y el micro-controlador posible. Después procedemos a referenciar y declarar las variables que usaremos en nuestro código.

* Después de esto se inicializa la función *Void setup* la cual se encarga de ejecutarse cada vez que inicia el programa, así buscando el puerto serial correspondiente e iniciandolo y buscando la cominicación con el giroscopio.

```cpp
[Header("Serial Coms")]
    public SerialController serialController;
    StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
    
    private float waitTime = 0.060f;
    private float time = 0.0f;
    
    public string message;
    string catcher;

```

* Después de esto se inicializa la función *Void setup* la cual se encarga de ejecutarse cada vez que inicia el programa, así buscando el puerto serial correspondiente e iniciandolo y buscando la cominicación con el giroscopio.

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
