using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dialogue
{
    public sealed class DialogueState : IDisposable
    {
        private DialogueLine[] _lines;
        private int _index;
        private TaskCompletionSource<bool> _completion;
        public bool IsActive => _completion != null;
        public bool BlocksGameplay { get; private set; }
        public Transform Source { get; private set; }
        public bool TracksSource { get; private set; }
        public DialogueLine Current => IsActive ? _lines[_index] : default;
        public int StartedFrame { get; private set; }
        public event Action Changed;

        public async Task PlayAsync(IReadOnlyList<DialogueLine> lines, bool blocksGameplay,
            Transform source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsActive) throw new InvalidOperationException("A dialogue is already active.");
            if (lines == null || lines.Count == 0) throw new ArgumentException("Dialogue requires lines.");
            var copy = new DialogueLine[lines.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i].Text))
                    throw new ArgumentException("Dialogue contains an empty line.");
                copy[i] = lines[i];
            }
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _lines = copy;
            _index = 0;
            _completion = completion;
            BlocksGameplay = blocksGameplay;
            Source = source;
            TracksSource = source != null;
            StartedFrame = UnityEngine.Time.frameCount;
            try
            {
                Changed?.Invoke();
                using (cancellationToken.Register(() => completion.TrySetCanceled()))
                    await completion.Task;
            }
            finally
            {
                if (ReferenceEquals(_completion, completion)) Clear();
            }
        }

        public void Advance()
        {
            if (!IsActive) return;
            if (_index + 1 < _lines.Length)
            { _index++; Changed?.Invoke(); return; }
            TaskCompletionSource<bool> completion = _completion;
            Clear();
            completion.TrySetResult(true);
        }

        public void Close()
        {
            if (!IsActive) return;
            TaskCompletionSource<bool> completion = _completion;
            Clear();
            completion.TrySetCanceled();
        }

        private void Clear()
        {
            _completion = null;
            _lines = null;
            _index = 0;
            BlocksGameplay = false;
            Source = null;
            TracksSource = false;
            Changed?.Invoke();
        }

        public void Dispose() => Close();
    }
}
