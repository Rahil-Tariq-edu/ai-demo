import { Link, Route, Routes, Navigate } from 'react-router-dom'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Chat from './pages/Chat'

export default function App() {
  return (
    <div className="h-full flex flex-col">
      <nav className="border-b px-4 py-2 flex gap-4 items-center">
        <Link to="/" className="font-semibold">KB Chatbot</Link>
        <Link to="/dashboard">Dashboard</Link>
        <Link to="/chat">Chat</Link>
      </nav>
      <div className="flex-1">
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/chat" element={<Chat />} />
        </Routes>
      </div>
    </div>
  )
}

