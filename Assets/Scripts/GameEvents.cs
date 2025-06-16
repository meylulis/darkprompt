using System;

/// <summary>
/// Статический класс для управления глобальными игровыми событиями.
/// Позволяет разным частям игры общаться друг с другом, не имея прямых ссылок.
/// </summary>
public static class GameEvents
{
    // Событие, которое вызывается, когда миссия/тест успешно пройдена на 100%.
    // Передает ID пройденной миссии.
    public static event Action<string> OnMissionCompleted;

    /// <summary>
    /// Метод для вызова события о прохождении миссии.
    /// </summary>
    /// <param name="missionId">ID пройденной миссии.</param>
    public static void MissionCompleted(string missionId)
    {
        // Вызываем событие, если на него есть подписчики
        OnMissionCompleted?.Invoke(missionId);
    }
} 