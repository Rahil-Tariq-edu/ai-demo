import { useState } from 'react'
import { Citation } from '../components/CitationList'

export function useChatState() {
  const [conversationId, setConversationId] = useState<string | null>(null)
  const [messages, setMessages] = useState<{ role: 'User'|'Assistant'|'System', text: string }[]>([])
  const [citations, setCitations] = useState<Citation[]>([])
  return { conversationId, setConversationId, messages, setMessages, citations, setCitations }
}

