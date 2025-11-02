import React, { useEffect, useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Header from "../../components/Header";

export default function StaiTest() {
    const { id } = useParams();
    const [searchParams] = useSearchParams();
    const sessionId = searchParams.get("sessionId");
    const { token } = useAuth();
    const navigate = useNavigate();

    const [age, setAge] = useState("");
    const [answers, setAnswers] = useState(Array(40).fill(null));

    const QUESTIONS = [
        "Я спокоен",
        "Мне ничто не угрожает",
        "Я нахожусь в напряжении",
        "Я внутренне скован",
        "Я чувствую себя свободно",
        "Я расстроен",
        "Меня волнуют возможные неудачи",
        "Я ощущаю душевный покой",
        "Я встревожен",
        "Я испытываю чувство внутреннего удовлетворения",
        "Я уверен в себе",
        "Я нервничаю",
        "Я не нахожу себе места",
        "Я взвинчен",
        "Я не чувствую скованности, напряжения",
        "Я доволен",
        "Я озабочен",
        "Я слишком возбужден и мне не по себе",
        "Мне радостно",
        "Мне приятно",
        "У меня бывает приподнятое настроение",
        "Я бываю раздражительным",
        "Я легко расстраиваюсь",
        "Я хотел бы быть таким же удачливым, как и другие",
        "Я сильно переживаю неприятности и долго не могу о них забыть",
        "Я чувствую прилив сил и желание работать",
        "Я спокоен, хладнокровен и собран",
        "Меня тревожат возможные трудности",
        "Я слишком переживаю из-за пустяков",
        "Я бываю вполне счастлив",
        "Я все принимаю близко к сердцу",
        "Мне не хватает уверенности в себе",
        "Я чувствую себя беззащитным",
        "Я стараюсь избегать критических ситуаций и трудностей",
        "У меня бывает хандра",
        "Я бываю доволен",
        "Всякие пустяки отвлекают и волнуют меня",
        "Бывает, что я чувствую себя неудачником",
        "Я уравновешенный человек",
        "Меня охватывает беспокойство, когда я думаю о своих делах и заботах"
    ];

    const OPTIONS = [
        { value: 1, label: "Никогда" },
        { value: 2, label: "Почти никогда" },
        { value: 3, label: "Часто" },
        { value: 4, label: "Почти всегда" },
    ];

    // Загружаем сохранённые ответы
    useEffect(() => {
        const saved = JSON.parse(localStorage.getItem(`stai-${sessionId}`));
        if (saved) {
            setAnswers(saved.answers || Array(40).fill(null));
            setAge(saved.age || "");
        }
    }, [sessionId]);

    // Сохраняем прогресс
    useEffect(() => {
        localStorage.setItem(`stai-${sessionId}`, JSON.stringify({ answers, age }));
    }, [answers, age, sessionId]);

    const handleAnswer = (qIndex, value) => {
        const updated = [...answers];
        updated[qIndex] = value;
        setAnswers(updated);
    };

    const handleSubmit = async () => {
        if (!age || isNaN(age) || age < 13 || age > 99) {
            alert("Введите корректный возраст (от 13 до 99 лет)");
            return;
        }
        if (answers.includes(null)) {
            alert("Пожалуйста, ответьте на все 40 вопросов");
            return;
        }

        const dto = { age: parseInt(age), answers };

        const res = await fetch(
            `${import.meta.env.VITE_MAIN_URL}/Tests/${sessionId}/stai`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify(dto),
            }
        );

        if (res.ok) {
            localStorage.removeItem(`stai-${sessionId}`);
            navigate(`/results/${sessionId}`);
        } else {
            const err = await res.text();
            console.error("❌ Ошибка сохранения:", err);
            alert("Ошибка при сохранении результатов");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="max-w-4xl mx-auto p-6">
                <h2 className="text-2xl font-bold mb-2 text-gray-800">
                    Тест Спилбергера–Ханина (STAI)
                </h2>
                <p className="text-gray-600 mb-6">
                    Выберите один вариант ответа для каждого утверждения:
                </p>

                {/* Возраст */}
                <div className="mb-8">
                    <label className="block mb-2 font-medium text-gray-700">
                        Ваш возраст:
                    </label>
                    <input
                        type="number"
                        value={age}
                        onChange={(e) => setAge(e.target.value)}
                        className="border rounded-lg px-3 py-2 w-32 text-center"
                        placeholder="Возраст"
                    />
                </div>

                {/* Вопросы */}
                <div className="space-y-6">
                    {QUESTIONS.map((q, i) => (
                        <div
                            key={i}
                            className="bg-white shadow p-5 rounded-xl border border-gray-200"
                        >
                            <p className="font-medium mb-4 text-gray-800">
                                {i + 1}. {q}
                            </p>

                            {/* вертикальные варианты с подписями */}
                            <div className="flex flex-col gap-2">
                                {OPTIONS.map((opt) => (
                                    <button
                                        key={opt.value}
                                        onClick={() => handleAnswer(i, opt.value)}
                                        className={`text-left px-4 py-3 rounded-lg border transition ${answers[i] === opt.value
                                            ? "bg-[color:var(--color-brand)] text-white border-[color:var(--color-brand)]"
                                            : "bg-gray-100 hover:bg-gray-200 border-gray-300 text-gray-800"
                                            }`}
                                    >
                                        {opt.value}. {opt.label}
                                    </button>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                {/* Кнопка завершения */}
                <button
                    onClick={handleSubmit}
                    className="mt-10 px-6 py-3 bg-[color:var(--color-brand)] text-white rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Завершить тест
                </button>
            </main>
        </div>
    );
}

