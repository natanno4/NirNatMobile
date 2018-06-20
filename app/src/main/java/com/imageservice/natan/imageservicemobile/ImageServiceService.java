package com.imageservice.natan.imageservicemobile;

import android.app.Service;
import android.arch.lifecycle.ViewModelProvider;
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
import android.support.v4.app.NotificationManagerCompat;
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

        Toast.makeText(this, "service is starting...", Toast.LENGTH_SHORT);
        this.theFilter.addAction("android.net.wifi.supplicant.CONNECTION_CHANGE");
        this.theFilter.addAction("android.net.wifi.STATE_CHANGE");
        this.wifiReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                WifiManager wifiManager = (WifiManager) context.getSystemService(context.WIFI_SERVICE);
                NetworkInfo networkInfo = intent.getParcelableExtra(WifiManager.EXTRA_NETWORK_INFO);
                if (networkInfo != null) {
                    if (networkInfo.getType() == ConnectivityManager.TYPE_WIFI) {
                        if (networkInfo.getState() == NetworkInfo.State.CONNECTED) {
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
        Toast.makeText(this, "service is stopping...", Toast.LENGTH_SHORT);
    }

    public void startTransfer() {
        final File[] pics = getAllCameraPhotos();
        if(pics == null) {
            return;
        }
        final int length = pics.length;
        if(pics != null) {
            final NotificationManagerCompat notificationManager = NotificationManagerCompat.from(this);
            final int notify_id = 1;
            final NotificationCompat.Builder builder = new NotificationCompat.Builder(this, "default");
            new Thread(new Runnable() {
                @Override
                public void run() {
                    builder.setSmallIcon(R.drawable.ic_launcher_background);
                    builder.setContentTitle("Picture Transfer")
                            .setContentText("Transfer in progress")
                            .setPriority(NotificationCompat.PRIORITY_LOW);
                    for (; count < length; ) {
                        if (count == length / 2) {
                            builder.setContentText("Half way through");
                            notificationManager.notify(notify_id, builder.build());
                        }
                        builder.setProgress(length, count, false);
                        notificationManager.notify(notify_id, builder.build());
                    }
                    // At the End
                    builder.setContentText("Download complete")
                            .setProgress(0, 0, false);
                    notificationManager.notify(notify_id, builder.build());
                }
            }).start();
            new Thread(new Runnable() {
                @Override
                public void run() {
                    for (File pic : pics) {
                        count++;
                        try {
                            InetAddress serverAddr = InetAddress.getByName("10.0.2.2");
                            Socket socket = new Socket(serverAddr, 2500);
                            try {
                                OutputStream outputStream = socket.getOutputStream();
                                InputStream input = socket.getInputStream();
                                FileInputStream fis = new FileInputStream(pic);
                                Bitmap bm = BitmapFactory.decodeStream(fis);
                                byte[] imgbyte = getBytesFromBitmap(bm);
                                outputStream.write(pic.getName().toString().getBytes());
                                outputStream.flush();
                                sleep(200);
                                byte[] confirmByte = new byte[1];
                                input.read(confirmByte, 0 ,confirmByte.length);
                                if((int) confirmByte[0] == 1) {
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
                }
            }).start();
        }
    }

    public File[] getAllCameraPhotos() {
        File dcim = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);
        if(dcim == null) {
            return null;
        }
        File[] files =  dcim.listFiles();
        List<File> pics = new ArrayList<File>();
        for(File f : files) {
            if(f.isDirectory()) {
                HandleDir(pics, f);
            }
            else {
                pics.add(f);
            }
        }
        File[] temp = new File[pics.size()];
        int c = 0;
        for (File f : pics) {
            temp[c] = f;
            c++;
        }
        return temp;
    }

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

    public byte[] getBytesFromBitmap(Bitmap bitmap) {
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 70, stream);
        return stream.toByteArray();
    }
}
