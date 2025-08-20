export default function MessageBubble({ role, text }: { role: 'User' | 'Assistant' | 'System', text: string }) {
  return (
    <div className={`p-3 rounded ${role === 'User' ? 'bg-blue-100 self-end' : 'bg-gray-100 self-start'} max-w-[80%] whitespace-pre-wrap`}>{text}</div>
  )
}

