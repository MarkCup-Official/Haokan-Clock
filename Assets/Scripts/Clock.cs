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

    [Header("倒计时设置")]
    public float time = 0;

    [Header("只读信息")]
    [Tooltip("计算出的时间戳（Unix时间戳，秒）")]
    [SerializeField]
    private long displayTimestamp = 0;

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

    // 计算出的时间戳（Unix时间戳，秒）
    private long targetTimestamp = 0;
    
    // 是否已经开始倒计时
    private bool hasStarted = false;

    public int Hours => _hours;
    public int Minutes => _minutes;
    public int Seconds => _seconds;
    public int Milliseconds => _milliseconds;

    void OnValidate()
    {
        // 当在检查器中修改值时，重新计算时间戳
        CalculateTimestamp();
    }

    void Start()
    {
        CalculateTimestamp();
    }

    void CalculateTimestamp()
    {
        try
        {
            DateTime targetDateTime = new DateTime(targetYear, targetMonth, targetDay, targetHour, targetMinute, targetSecond);
            targetTimestamp = ((DateTimeOffset)targetDateTime).ToUnixTimeSeconds();
            displayTimestamp = targetTimestamp;
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.LogWarning($"无效的日期时间设置: {targetYear}-{targetMonth}-{targetDay} {targetHour}:{targetMinute}:{targetSecond}");
        }
    }

    void Update()
    {
        // 检查当前系统时间是否已经超过目标时间戳
        long currentTimestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        
        if (currentTimestamp >= targetTimestamp)
        {
            if (!hasStarted)
            {
                hasStarted = true;
                Debug.Log($"倒计时开始！目标时间: {targetYear}-{targetMonth}-{targetDay} {targetHour}:{targetMinute}:{targetSecond}");
            }
            
            time -= Time.deltaTime * 30;
            
            // 更新只读属性显示值
            _hours = Mathf.FloorToInt((time+60) / 3600f);
            _minutes = Mathf.FloorToInt((time+60) / 60f) % 60;
            _seconds = Mathf.FloorToInt(time) % 60;
            _milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
        }
        else
        {
            // 还未到达目标时间，显示等待状态
            long remainingSeconds = targetTimestamp - currentTimestamp;
            _hours = Mathf.FloorToInt(remainingSeconds / 3600f);
            _minutes = Mathf.FloorToInt(remainingSeconds / 60f) % 60;
            _seconds = Mathf.FloorToInt(remainingSeconds) % 60;
            _milliseconds = 0;
        }
    }
}
