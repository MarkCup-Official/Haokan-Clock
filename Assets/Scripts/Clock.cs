using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [Header("时间设置")]
    [Tooltip("目标年份")]
    public int targetYear = 2024;
    
    [Tooltip("目标月份 (1-12)")]
    [Range(1, 12)]
    public int targetMonth = 1;
    
    [Tooltip("目标日期 (1-31)")]
    [Range(1, 31)]
    public int targetDay = 1;
    
    [Tooltip("目标小时 (0-23)")]
    [Range(0, 23)]
    public int targetHour = 0;
    
    [Tooltip("目标分钟 (0-59)")]
    [Range(0, 59)]
    public int targetMinute = 0;
    
    [Tooltip("目标秒数 (0-59)")]
    [Range(0, 59)]
    public int targetSecond = 0;

    [Header("只读信息")]
    [Tooltip("计算出的时间戳（Unix时间戳，秒）")]
    [SerializeField]
    public long displayTimestamp = 0;

    [Tooltip("小时")]
    [SerializeField]
    private int _hours = 0;
    
    [Tooltip("分钟")]
    [SerializeField]
    private int _minutes = 0;
    
    [Tooltip("秒")]
    [SerializeField]
    private int _seconds = 0;
    
    [Tooltip("毫秒")]
    [SerializeField]
    private int _milliseconds = 0;

    // 目标日期时间
    private DateTime targetDateTime;

    public int Hours => _hours;
    public int Minutes => _minutes;
    public int Seconds => _seconds;
    public int Milliseconds => _milliseconds;

    public bool carryShowMinute = true; // 进位显示分钟, 20秒显示一分钟

    public long currentTimestamp;

    void OnValidate()
    {
        // 当在检查器中修改值时，重新计算目标时间
        CalculateTargetTime();
    }

    void Start()
    {
        CalculateTargetTime();
    }

    void CalculateTargetTime()
    {
        try
        {
            targetDateTime = new DateTime(targetYear, targetMonth, targetDay, targetHour, targetMinute, targetSecond);
            displayTimestamp = ((DateTimeOffset)targetDateTime).ToUnixTimeSeconds();
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.LogWarning($"无效的日期时间设置: {targetYear}-{targetMonth}-{targetDay} {targetHour}:{targetMinute}:{targetSecond}");
        }
    }

    void Update()
    {
        // 计算当前系统时间与目标时间的差值
        TimeSpan remaining = targetDateTime - DateTime.Now;

        currentTimestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

        if (remaining.TotalMilliseconds <= 0)
        {
            // 如果时间已到或已过，全部归零
            _hours = 0;
            _minutes = 0;
            _seconds = 0;
            _milliseconds = 0;
        }
        else
        {
            // 更新倒计时显示值
            // 使用 TotalHours 以便在超过24小时时显示总小时数
            _seconds = remaining.Seconds;
            int totalSeconds = (int)remaining.TotalSeconds;
            _hours = carryShowMinute ? (totalSeconds+60)/60/60 : (int)remaining.TotalHours;
            _minutes = carryShowMinute ? (totalSeconds+60)/60%60 : remaining.Minutes;
            _milliseconds = remaining.Milliseconds;
        }
    }
}