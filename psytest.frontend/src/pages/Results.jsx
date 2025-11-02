import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import Header from "../components/Header";

export default function Result() {
    const { sessionId } = useParams();
    const { token } = useAuth();
    const [result, setResult] = useState(null);

    useEffect(() => {
        const fetchResult = async () => {
            const res = await fetch(`${import.meta.env.VITE_MAIN_URL}/Results/results/${sessionId}`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            if (res.ok) {
                const data = await res.json();
                setResult(data);
            } else {
                alert("Ошибка при загрузке результата");
            }
        };

        fetchResult();
    }, [sessionId, token]);

    const downloadDocx = () => {
        if (!result?.reportBytes) {
            alert("Файл отсутствует");
            return;
        }

        const byteCharacters = atob(result.reportBytes);
        const byteNumbers = Array.from(byteCharacters, c => c.charCodeAt(0));
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], {
            type: "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        });

        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = `report_${sessionId}.docx`;
        link.click();
    };

    if (!result) return <div className="p-6">Загрузка...</div>;

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="max-w-3xl mx-auto p-6 bg-white shadow rounded-xl mt-6">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-2xl font-bold">Результат</h2>
                    {result.reportBytes && (
                        <button
                            onClick={downloadDocx}
                            className="px-4 py-2 bg-[color:var(--color-brand)] text-white rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                        >
                            Скачать отчёт (DOCX)
                        </button>
                    )}
                </div>

                
                <p className="whitespace-pre-line text-gray-700 mb-6">{result.resultText}</p>

                {/* 🔹 Блок с графиками */}
                {result.images && result.images.length > 0 && (
                    <div className="space-y-6 mb-6">
                        {result.images.map((img, idx) => (
                            <div key={idx} className="flex justify-center">
                                <img
                                    src={`data:image/png;base64,${img}`}
                                    alt={`Image ${idx + 1}`}
                                    className="rounded-lg shadow-lg max-w-full"
                                />
                            </div>
                        ))}
                    </div>
                )}

                
            </main>
        </div>
    );
}
