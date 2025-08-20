import { useEffect, useState } from 'react'
import { api } from '../api/client'
import MessageBubble from '../components/MessageBubble'
import CitationList, { Citation } from '../components/CitationList'

type Msg = { id: string, conversationId: string, role: 'User'|'Assistant'|'System', text: string, createdAt: string }

export default function Chat() {
  const [conversationId, setConversationId] = useState<string | null>(null)
  const [messages, setMessages] = useState<{ role: 'User'|'Assistant'|'System', text: string }[]>([])
  const [input, setInput] = useState('What is this app?')
  const [citations, setCitations] = useState<Citation[]>([])
  const [busy, setBusy] = useState(false)

  const send = async () => {
    if (!input.trim()) return
    setBusy(true)
    const userMsg = { role: 'User' as const, text: input }
    setMessages(prev => [...prev, userMsg])
    setInput('')
    const { data } = await api.post('/api/chat/ask', { conversationId, message: userMsg.text })
    setConversationId(data.conversationId)
    setMessages(prev => [...prev, { role: 'Assistant', text: data.answer }])
    setCitations(data.citations)
    setBusy(false)
  }

  useEffect(() => { setMessages([{ role:'System', text:'Ask a question about your knowledge base.' }]) }, [])

  return (
    <div className="h-full grid grid-rows-[1fr_auto]">
      <div className="p-4 flex flex-col gap-3 overflow-auto">
        {messages.map((m, i) => (<MessageBubble key={i} role={m.role} text={m.text} />))}
        <CitationList citations={citations} />
      </div>
      <div className="border-t p-3 flex gap-2">
        <input className="border p-2 flex-1" placeholder="Type your question..." value={input} onChange={e=>setInput(e.target.value)} onKeyDown={e=>e.key==='Enter'&&send()} />
        <button className="bg-black text-white px-4 disabled:opacity-50" disabled={busy} onClick={send}>Send</button>
      </div>
    </div>
  )
}

