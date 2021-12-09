using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRecorder : MonoBehaviour
{
    private List<Record> m_VersionHistory = new List<Record>();
    private Record m_CurrentVersion = new Record();
    private float m_LastRecordTime = -1;
    private int m_VersionIndex = -1;

    public Record Redo()
    {
        if (m_VersionIndex < m_VersionHistory.Count - 1) m_VersionIndex++;
        return m_VersionHistory[m_VersionIndex];
    }

    public Record Undo()
    {
        if (m_VersionIndex > 0) m_VersionIndex--;
        return m_VersionHistory[m_VersionIndex];
    }

    public void Record(Record newVersion)
    {
        if (Time.time - m_LastRecordTime > .5f)
        {
            if (m_VersionHistory.Count > 0 && m_VersionIndex < m_VersionHistory.Count - 1)
            {
                m_VersionHistory.RemoveRange(m_VersionIndex + 1, m_VersionHistory.Count - m_VersionIndex - 1);
            }
            m_VersionHistory.Add(newVersion);
            m_VersionIndex = m_VersionHistory.Count - 1;
        }
        else m_VersionHistory[m_VersionIndex] = newVersion;
        m_LastRecordTime = Time.time;
    }
}

public class Record
{
    public string text = "";
    public int stringPosition = 0;
    public int stringSelectPosition = 0;

    public Record()
    {
        text = "";
        stringPosition = 0;
        stringSelectPosition = 0;
    }

    public Record(string _text, int _stringPosition, int _stringSelectPosition)
    {
        text = _text;
        stringPosition = _stringPosition;
        stringSelectPosition = _stringSelectPosition;
    }
}
