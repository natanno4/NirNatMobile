package com.imageservice.natan.imageservicemobile;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.wifi.WifiManager;
import android.os.Environment;
import android.os.IBinder;
import android.support.annotation.Nullable;
import android.support.v4.app.NotificationCompat;
import android.util.Log;
import android.widget.Toast;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;

import static java.lang.Thread.sleep;

public class ImageServiceService extends Service {
    private BroadcastReceiver wifiReceiver;
    private IntentFilter theFilter;
    private int count = 0;

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        this.theFilter = new IntentFilter();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        //start service
        Toast.makeText(this, "service is starting...", Toast.LENGTH_SHORT);
        this.theFilter.addAction("android.net.wifi.supplicant.CONNECTION_CHANGE");
        this.theFilter.addAction("android.net.wifi.STATE_CHANGE");
        //create wifi broadcast
        this.wifiReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                WifiManager wifiManager = (WifiManager) context.getSystemService(context.WIFI_SERVICE);
                NetworkInfo networkInfo = intent.getParcelableExtra(WifiManager.EXTRA_NETWORK_INFO);
                //check if wifi is connected
                if (networkInfo != null) {
                    if (networkInfo.getType() == ConnectivityManager.TYPE_WIFI) {
                        if (networkInfo.getState() == NetworkInfo.State.CONNECTED) {
                            //wifi connected-start transfer.
                            startTransfer();
                        }
                    }
                }
            }
        };
        this.registerReceiver(this.wifiReceiver, theFilter);
        return START_STICKY;
    }

    public void onDestroy() {
        super.onDestroy();
        Toast.makeText(this, "service is stopping...", Toast.LENGTH_SHORT);
    }


    /**
     * start transfer the images to the server.
     */
    public void startTransfer() {
        //get images in DCIM
        final File[] pics = getAllCameraPhotos();
        if(pics == null) {
            return;
        }
        //save number of images.
        final int length = pics.length;

        //create notification bar.
        try {
            final NotificationManager notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
            final NotificationCompat.Builder builder;
            final int notify_id = 1;
            if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                int importance = NotificationManager.IMPORTANCE_LOW;
                NotificationChannel notificationChannel = new NotificationChannel("ImageServiceAppChannel", "Image Service App Channel", importance);
                notificationChannel.enableLights(true);
                notificationChannel.enableVibration(true);
                notificationChannel.setVibrationPattern(new long[]{100, 200, 300, 400, 500, 400, 300, 200, 400});
                notificationManager.createNotificationChannel(notificationChannel);
                builder = new NotificationCompat.Builder(this, "ImageServiceAppChannel");

            } else {
                builder = new NotificationCompat.Builder(this, "default");

            }

            new Thread(new Runnable() {
                @Override
                public void run() {
                    //set start bar information
                    builder.setSmallIcon(R.drawable.ic_launcher_background);
                    builder.setContentTitle("Picture Transfer")
                            .setContentText("Transfer in progress")
                            .setPriority(NotificationCompat.PRIORITY_LOW);
                    for (File pic : pics) {
                        //images wiil be send
                        count++;
                        if (count == length / 2) {
                            //set the half way bar notification information
                            builder.setContentText("Half way through");
                        }
                        //show progres
                        builder.setProgress(length, count, false);
                        notificationManager.notify(notify_id, builder.build());
                        try {
                            sleep(500);
                            //connect to socket
                            InetAddress serverAddr = InetAddress.getByName("10.0.2.2");
                            Socket socket = new Socket(serverAddr, 2500);
                            try {
                                //get the output stream
                                OutputStream outputStream = socket.getOutputStream();
                                InputStream input = socket.getInputStream();
                                FileInputStream fis = new FileInputStream(pic);
                                Bitmap bm = BitmapFactory.decodeStream(fis);
                                //convert image to byte array
                                byte[] imgbyte = getBytesFromBitmap(bm);
                                //send image name
                                outputStream.write(pic.getName().toString().getBytes());
                                outputStream.flush();
                                sleep(200);
                                byte[] confirmByte = new byte[1];
                                //get confirmation that the server got the image name
                                input.read(confirmByte, 0, confirmByte.length);
                                if ((int) confirmByte[0] == 1) {
                                    //the server confirm it
                                    //send the image
                                    outputStream.write(imgbyte);
                                }
                                outputStream.flush();
                            } catch (Exception e) {
                                Log.e("TCP", "S: Error", e);
                            } finally {
                                socket.close();
                            }
                        } catch (Exception e) {
                            Log.e("TCP", "C: Error", e);
                        }
                    }
                    //set end of sending bar.
                    builder.setContentText("Download complete")
                            .setProgress(0, 0, false);
                    notificationManager.notify(notify_id, builder.build());
                }
            }).start();
        }catch (Exception e) {
            Log.e("error", "C: Error", e);
        }
    }

    /**
     * returns a list of images as filed from the DCIm folder.
     * @return list of images in DCIM.
     */
    public File[] getAllCameraPhotos() {
        File dcim = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);
        if(dcim == null) {
            return null;
        }
        //get files in DCIM
        File[] files =  dcim.listFiles();
        List<File> pics = new ArrayList<File>();
        //find images in DCIM
        for(File f : files) {
            if(f.isDirectory()) {
                //handle sub dir files
                HandleDir(pics, f);
            }
            else {
                //add image to list
                pics.add(f);
            }
        }
        File[] temp = new File[pics.size()];
        int c = 0;
        //create an array of file instead of list
        for (File f : pics) {
            temp[c] = f;
            c++;
        }
        return temp;
    }

    /**
     * find and save images from the d irectory. if a directory is fond ,call
     * the function again.
     * @param files the list to save the images.
     * @param dir the directory.
     */
    private void HandleDir(List<File> files, File dir) {
        File[] temp = dir.listFiles();
        for(File f : temp) {
            if(f.isDirectory()) {
                HandleDir(files, f);
            }
            else {
                files.add(f);
            }
        }
    }

    /**
     * convert an image as bitye array.
     * @param bitmap the image
     * @return the byte arrayt.
     */
    public byte[] getBytesFromBitmap(Bitmap bitmap) {
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 70, stream);
        return stream.toByteArray();
    }
}
