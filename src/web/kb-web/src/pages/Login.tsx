import { useState } from 'react'
import { api } from '../api/client'

export default function Login() {
  const [email, setEmail] = useState('admin@example.com')
  const [password, setPassword] = useState('admin123')
  const [error, setError] = useState('')

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const { data } = await api.post('/api/auth/login', { email, password })
      localStorage.setItem('jwt', data.token)
      window.location.href = '/dashboard'
    } catch (err:any) {
      setError(err?.response?.data?.error || 'Login failed')
    }
  }

  return (
    <div className="max-w-sm mx-auto p-6">
      <h1 className="text-xl font-semibold mb-4">Login</h1>
      <form onSubmit={onSubmit} className="flex flex-col gap-3">
        <input className="border p-2" placeholder="Email" value={email} onChange={e=>setEmail(e.target.value)} />
        <input type="password" className="border p-2" placeholder="Password" value={password} onChange={e=>setPassword(e.target.value)} />
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <button className="bg-black text-white py-2">Sign in</button>
      </form>
    </div>
  )
}

