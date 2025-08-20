export type Citation = { index: number, title: string, url: string, excerpt: string }

export default function CitationList({ citations }: { citations: Citation[] }) {
  if (!citations?.length) return null
  return (
    <div className="border-t mt-4 pt-2">
      <div className="font-semibold mb-2">Citations</div>
      <ul className="list-disc list-inside text-sm">
        {citations.map(c => (
          <li key={c.index}>
            [{c.index}] <a href={c.url || '#'} target="_blank" className="text-blue-700 underline">{c.title || 'Untitled'}</a>
          </li>
        ))}
      </ul>
    </div>
  )
}

