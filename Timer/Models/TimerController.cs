﻿using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace Kchary.Timer.Model
{
    public class TimerController
    {
        // 音程を表す周波数と長さ
        private readonly int cfreq = 262; // ド
        private readonly int efreq = 330; // ミ
        private readonly int gfreq = 392; // ソ
        private readonly int duration = 500;

        // Timerイベント
        public delegate void TimerEventHandler(TimerValue _timerValue);
        public event TimerEventHandler TimerEvent;

        // タイマー内部の管理変数
        public class TimerValue
        {
            public int Minute { get; set; }
            public int Second { get; set; }

            public TimerValue(int _minute, int _second)
            {
                Minute = _minute;
                Second = _second;
            }
        }

        // 変数と定数
        private readonly int MAX_TIMER_VALUE = 59;
        private readonly int MIN_TIMER_VALUE = 0;
        public TimerValue TimerMinuteAndSecond;

        // コンストラクタ
        public TimerController()
        {
            // デフォルトの値をセット
            const int _defaultMinuteTimer = 0;
            const int _defaultSecondTimer = 0;
            TimerMinuteAndSecond = new TimerValue(_defaultMinuteTimer, _defaultSecondTimer);
        }

        /// <summary>
        /// １分タイマー値にプラス
        /// </summary>
        public string PlusMinute()
        {
            // タイマー値の最大値より小さい場合は＋１
            if (TimerMinuteAndSecond.Minute < MAX_TIMER_VALUE)
            {
                TimerMinuteAndSecond.Minute += 1;
            }

            return GetStringValueFromTimerValue(TimerMinuteAndSecond.Minute);
        }

        /// <summary>
        /// １分タイマー値からマイナス
        /// </summary>
        public string MinusMinute()
        {
            // タイマー値の最小値より大きい場合は-１
            if (TimerMinuteAndSecond.Minute > MIN_TIMER_VALUE)
            {
                TimerMinuteAndSecond.Minute -= 1;
            }

            return GetStringValueFromTimerValue(TimerMinuteAndSecond.Minute);
        }

        /// <summary>
        /// １秒タイマー値にプラス
        /// </summary>
        public string PlusSecond()
        {
            // タイマー値の最大値より小さい場合は＋１
            if (TimerMinuteAndSecond.Second < MAX_TIMER_VALUE)
            {
                TimerMinuteAndSecond.Second += 1;
            }

            return GetStringValueFromTimerValue(TimerMinuteAndSecond.Second);
        }

        /// <summary>
        /// １秒タイマー値からマイナス
        /// </summary>
        public string MinusSecond()
        {
            // タイマー値の最小値より大きい場合は-１
            if (TimerMinuteAndSecond.Second > MIN_TIMER_VALUE)
            {
                TimerMinuteAndSecond.Second -= 1;
            }

            return GetStringValueFromTimerValue(TimerMinuteAndSecond.Second);
        }

        /// <summary>
        /// タイマー値を文字列に変換する
        /// </summary>
        /// <param name="_timerValue">タイマー値</param>
        /// <returns>タイマー値を文字列に変換した結果</returns>
        public string GetStringValueFromTimerValue(int _timerValue)
        {
            string _timerStringValue;

            // タイマーの値が10以下の場合は2桁目に0をつける
            if (_timerValue < 10)
            {
                _timerStringValue = "0" + _timerValue.ToString();
            }
            else
            {
                _timerStringValue = _timerValue.ToString();
            }

            return _timerStringValue;
        }

        private System.Timers.Timer TimerCount;
        /// <summary>
        /// タイマーをスタート
        /// </summary>
        public void StartTimer()
        {
            // 設定されたタイマーの値が分、秒どちらも0以下の場合は何もしない
            if (TimerMinuteAndSecond.Minute <= MIN_TIMER_VALUE && TimerMinuteAndSecond.Second <= MIN_TIMER_VALUE)
            {
                return;
            }

            if (TimerCount != null)
            {
                return;
            }

            TimerCount = new System.Timers.Timer(1000); // 1秒ごとの処理にセット

            // タイマーでの処理をイベントハンドラーとして登録
            TimerCount.Elapsed += new ElapsedEventHandler(OnElapsed_TimersTimer);

            // タイマーを開始
            TimerCount.Start();
        }

        /// <summary>
        /// タイマー動作中の動作
        /// </summary>
        private void OnElapsed_TimersTimer(object _sender, ElapsedEventArgs _e)
        {
            if (TimerMinuteAndSecond.Minute > MIN_TIMER_VALUE && TimerMinuteAndSecond.Second == MIN_TIMER_VALUE)
            {
                // 1分減らして59秒にする
                TimerMinuteAndSecond.Minute -= 1;
                TimerMinuteAndSecond.Second = MAX_TIMER_VALUE;
            }
            else
            {
                // 1秒減らす
                TimerMinuteAndSecond.Second -= 1;
            }

            // UIに反映(別スレッドなので、Dispatcherを利用)
            var dispatcher = App.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                TimerEvent(TimerMinuteAndSecond);
            }));

            // どちらも0になった場合はタイマー終了
            if (TimerMinuteAndSecond.Minute == MIN_TIMER_VALUE && TimerMinuteAndSecond.Second == MIN_TIMER_VALUE)
            {
                // タイマーを終了
                TimerCount.Stop();
                TimerCount.Dispose();
                TimerCount = null;

                // ドミソミドと音を鳴らす
                BeepSound();

                return;
            }
        }

        /// <summary>
        /// タイマーをストップ
        /// </summary>
        public void StopTimer()
        {
            if (TimerCount == null)
            {
                return;
            }

            TimerCount.Stop();
            TimerCount.Dispose();
            TimerCount = null;
        }

        /// <summary>
        /// タイマーをリセット
        /// </summary>
        public void ResetTimer()
        {
            // タイマーが動作中の場合
            if (TimerCount != null)
            {
                // タイマーを停止
                TimerCount.Stop();
                TimerCount.Dispose();
                TimerCount = null;
            }

            // タイマーを最小値に戻す
            TimerMinuteAndSecond.Minute = MIN_TIMER_VALUE;
            TimerMinuteAndSecond.Second = MIN_TIMER_VALUE;

            // UIに反映
            TimerEvent(TimerMinuteAndSecond);
        }

        /// <summary>
        /// ドミソミドと音を鳴らす
        /// </summary>
        private void BeepSound()
        {
            Beep(cfreq, duration); // ド
            Beep(efreq, duration); // ミ
            Beep(gfreq, duration); // ソ
            Beep(efreq, duration); // ミ
            Beep(cfreq, duration); // ド
        }

        // Beep音の作成(DLLの読み込み)
        [DllImport("Kernel32.dll")]
        public static extern bool Beep(int _dwFreq, int _dwDuration);
    }

}
