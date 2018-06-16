package com.imageservice.natan.imageservicemobile;

import android.app.Service;
import android.arch.lifecycle.ViewModelProvider;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Bitmap;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.wifi.WifiManager;
import android.os.Environment;
import android.os.IBinder;
import android.support.annotation.Nullable;
import android.util.Log;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.Socket;

public class ImageServiceService extends Service {
    private BroadcastReceiver wifiReceiver;
    private Socket socket;
    private OutputStream outputStream;

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        Toast.makeText(this, "service is starting...", Toast.LENGTH_SHORT);
        try {
            InetAddress serverAddr = InetAddress.getByName("10.0.0.2");
            this.socket = new Socket(serverAddr, 8600);
        } catch (Exception e) {
            Log.e("TCP", "c:Error", e);
        }
        final IntentFilter theFilter = new IntentFilter();
        theFilter.addAction("android.new.wifi.supplicant.CONNECTION_Change");
        theFilter.addAction("android.net.wifi.STATE_CHANGE");
        this.wifiReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                WifiManager wifiManager = (WifiManager) context.getSystemService(context.WIFI_SERVICE);
                NetworkInfo networkInfo = intent.getParcelableExtra(WifiManager.EXTRA_NETWORK_INFO);
                if(networkInfo != null) {
                    if(networkInfo.getType() == ConnectivityManager.TYPE_WIFI) {
                        if(networkInfo.getState() == NetworkInfo.State.CONNECTED) {
                            startTransfer();
                        }
                    }
                }
            }
        };
        this.registerReceiver(this.wifiReceiver, theFilter);

    }

    public void onDestroy() {
        Toast.makeText(this, "service is stopping...", Toast.LENGTH_SHORT);
    }

    public void startTransfer() {
        
    }

    public File[] getAllCameraPhotos() {
        File dcim = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);
        if(dcim == null) {
            return null;
        }
        return dcim.listFiles();
    }

    public byte[] getBytesFromBitmap(Bitmap bitmap) {
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 70, stream);
        return stream.toByteArray();
    }






}
