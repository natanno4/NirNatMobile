package com.imageservice.natan.imageservicemobile;

import android.app.Service;
import android.content.Intent;
import android.os.IBinder;
import android.support.annotation.Nullable;
import android.widget.Toast;

public class ImageServiceService extends Service {
    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    public int onStartCommand(Intent intent, int flag, int startId) {

        Toast.makeText(this, "service is starting...", Toast.LENGTH_SHORT);
        return START_STICKY;
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

    public void onDestroy() {
        Toast.makeText(this, "service is stopping...", Toast.LENGTH_SHORT);
    }
}
