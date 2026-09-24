import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

const API_URL = 'http://213.165.61.19:8087/api/Selection';
const SKIS_URL = 'http://213.165.61.19:8087/api/Skis';
const GRINDS_URL = 'http://213.165.61.19:8087/api/StoneGrinds';
const CATEGORY_RU = {
  'Glide': 'Скольжение',
  'Powder': 'Порошок',
  'Grip': 'Держание'
};
const TYPE_RU = {
  'Base': 'Основа',
  'Finish': 'Финиш',
  'Universal': 'Универсальная'
};

function App() {
  const [activeTab, setActiveTab] = useState('selection');
  const [form, setForm] = useState({
    latitude: null,
    longitude: null,
    airTemp: '',
    humidity: '',
    windSpeed: '',
    isSunny: false,
    snowType: 'All',
    trackType: 'All',
    style: 'Classic'
  });

  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [geoStatus, setGeoStatus] = useState('');

  const [skis, setSkis] = useState([]);
  const [skisLoading, setSkisLoading] = useState(false);
  const [skisError, setSkisError] = useState('');

  const [stoneGrinds, setStoneGrinds] = useState([]);
  const [showAddSki, setShowAddSki] = useState(false);
  const [skiForm, setSkiForm] = useState(getEmptySkiForm());
  const [skiFormError, setSkiFormError] = useState('');
  const [skiSaving, setSkiSaving] = useState(false);
  const [waxes, setWaxes] = useState([]);
  const [waxesLoading, setWaxesLoading] = useState(false);
  const [waxesError, setWaxesError] = useState('');
  const [showAddWax, setShowAddWax] = useState(false);
  const [waxForm, setWaxForm] = useState(getEmptyWaxForm());
  const [waxFormError, setWaxFormError] = useState('');
  const [waxSaving, setWaxSaving] = useState(false);
  const [editingWaxId, setEditingWaxId] = useState(null);
  const [editingSkiId, setEditingSkiId] = useState(null);


  const loadSkis = async () => {
    setSkisLoading(true);
    setSkisError('');
    try {
      const res = await axios.get(SKIS_URL);
      setSkis(res.data);
    } catch (err) {
      setSkisError(err.response?.data?.error || err.message);
    } finally {
      setSkisLoading(false);
    }
  };

  const loadStoneGrinds = async () => {
    try {
      const res = await axios.get(GRINDS_URL);
      setStoneGrinds(res.data);
    } catch (err) {
      console.error('Ошибка загрузки штайншлифтов:', err);
    }
  };

  const loadWaxes = async () => {
  setWaxesLoading(true);
  setWaxesError('');
  try {
    const res = await axios.get('http://213.165.61.19:8087/api/Waxes');
    setWaxes(res.data);
  } catch (err) {
    setWaxesError(err.response?.data?.error || err.message);
  } finally {
    setWaxesLoading(false);
  }
};

useEffect(() => {
  if (activeTab === 'skis') {
    if (skis.length === 0) loadSkis();
    if (stoneGrinds.length === 0) loadStoneGrinds();
  }
  if (activeTab === 'waxes' && waxes.length === 0) {
    loadWaxes();
  }
  // eslint-disable-next-line react-hooks/exhaustive-deps
}, [activeTab]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({ ...form, [name]: type === 'checkbox' ? checked : value });
  };

  const getLocation = () => {
    if (!navigator.geolocation) {
      setGeoStatus('Геолокация не поддерживается браузером');
      return;
    }
    setGeoStatus('Определяем...');
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const { latitude, longitude } = pos.coords;
        setForm((f) => ({ ...f, latitude, longitude }));
        setGeoStatus(`Координаты: ${latitude.toFixed(4)}, ${longitude.toFixed(4)}`);
      },
      (err) => setGeoStatus('Ошибка: ' + err.message),
      { enableHighAccuracy: true, timeout: 10000 }
    );
  };

  const clearLocation = () => {
    setForm((f) => ({ ...f, latitude: null, longitude: null }));
    setGeoStatus('');
  };

  const handleSkiFormChange = (e) => {
    const { name, value, type, checked } = e.target;
    setSkiForm((f) => ({ ...f, [name]: type === 'checkbox' ? checked : value }));
  };

  const resetSkiForm = () => {
    setSkiForm(getEmptySkiForm());
    setSkiFormError('');
    setEditingSkiId(null);
  };


const openEditSki = (ski) => {
  setSkiForm({
    brand: ski.brand || '',
    model: ski.model || '',
    year: ski.year || '',
    style: ski.style || 'Classic',
    length: ski.length || '',
    profile: ski.profile || '',
    profileTempMin: ski.profileTempMin ?? '',
    profileTempMax: ski.profileTempMax ?? '',
    stiffnessValue: ski.stiffnessValue ?? '',
    stiffnessLabel: ski.stiffnessLabel || '',
    camberHeightMm: ski.camberHeightMm ?? '',
    hasSkin: ski.hasSkin || false,
    stoneGrindName: ski.stoneGrindName || ski.stoneGrind?.name || '',
    stoneGrindId: ski.stoneGrindId || '',
    notes: ski.notes || '',
    personalNotes: ski.personalNotes || ''
  });
  setEditingSkiId(ski.id);
  setShowAddSki(true);
};


const deleteSki = async (ski) => {
  const ok = window.confirm(
    `Удалить пару "${ski.brand} ${ski.model}"?\n\nВся история штайншлифтов тоже будет удалена.`
  );
  if (!ok) return;

  try {
    await axios.delete(`${SKIS_URL}/${ski.id}`);
    await loadSkis();
  } catch (err) {
    console.error(err);
    alert('Ошибка удаления: ' + (err.response?.data?.error || err.message));
  }
};

const handleWaxFormChange = (e) => {
  const { name, value } = e.target;
  setWaxForm((f) => ({ ...f, [name]: value }));
};

const resetWaxForm = () => {
  setWaxForm(getEmptyWaxForm());
  setWaxFormError('');
  setEditingWaxId(null);
};

const openEditWax = (wax) => {
  setWaxForm({
    name: wax.name || '',
    brand: wax.brand || '',
    category: wax.category || 'Glide',
    type: wax.type || 'Base',
    tempMin: wax.tempMin ?? '',
    tempMax: wax.tempMax ?? '',
    humidityMin: wax.humidityMin ?? '',
    humidityMax: wax.humidityMax ?? '',
    snowType: wax.snowType || 'All',
    trackType: wax.trackType || 'All',
    notes: wax.notes || '',
    warnings: wax.warnings || ''
  });
  setEditingWaxId(wax.id);
  setShowAddWax(true);
};

const saveWax = async () => {
  setWaxSaving(true);
  setWaxFormError('');
  try {
    const payload = {
      name: waxForm.name,
      brand: waxForm.brand,
      category: waxForm.category,
      type: waxForm.type,
      tempMin: waxForm.tempMin !== '' ? parseFloat(waxForm.tempMin) : 0,
      tempMax: waxForm.tempMax !== '' ? parseFloat(waxForm.tempMax) : 0,
      humidityMin: waxForm.humidityMin !== '' ? parseFloat(waxForm.humidityMin) : 0,
      humidityMax: waxForm.humidityMax !== '' ? parseFloat(waxForm.humidityMax) : 100,
      snowType: waxForm.snowType,
      trackType: waxForm.trackType,
      notes: waxForm.notes || null,
      warnings: waxForm.warnings || null
    };

    const url = 'http://213.165.61.19:8087/api/Waxes';
    if (editingWaxId) {
      await axios.put(`${url}/${editingWaxId}`, payload);
    } else {
      await axios.post(url, payload);
    }

    setShowAddWax(false);
    resetWaxForm();
    await loadWaxes();
  } catch (err) {
    console.error(err);
    setWaxFormError(err.response?.data?.error || JSON.stringify(err.response?.data) || err.message);
  } finally {
    setWaxSaving(false);
  }
};

const deleteWax = async (wax) => {
  const ok = window.confirm(`Удалить мазь "${wax.name}"?`);
  if (!ok) return;
  try {
    await axios.delete(`http://213.165.61.19:8087/api/Waxes/${wax.id}`);
    await loadWaxes();
  } catch (err) {
    alert('Ошибка удаления: ' + (err.response?.data?.error || err.message));
  }
};

const saveSki = async () => {
  setSkiSaving(true);
  setSkiFormError('');
  try {
    const payload = {
      brand: skiForm.brand,
      model: skiForm.model,
      year: skiForm.year ? parseInt(skiForm.year) : null,
      style: skiForm.style,
      length: skiForm.length ? parseInt(skiForm.length) : null,
      profile: skiForm.profile || null,
      profileTempMin: skiForm.profileTempMin !== '' ? parseFloat(skiForm.profileTempMin) : null,
      profileTempMax: skiForm.profileTempMax !== '' ? parseFloat(skiForm.profileTempMax) : null,
      stiffnessValue: skiForm.stiffnessValue !== '' ? parseFloat(skiForm.stiffnessValue) : null,
      stiffnessLabel: skiForm.stiffnessLabel || null,
      camberHeightMm: skiForm.camberHeightMm !== '' ? parseFloat(skiForm.camberHeightMm) : null,
      hasSkin: skiForm.hasSkin,
      stoneGrindName: skiForm.stoneGrindName || null,
      stoneGrindId: skiForm.stoneGrindId ? parseInt(skiForm.stoneGrindId) : null,
      notes: skiForm.notes || null,
      personalNotes: skiForm.personalNotes || null
    };

    if (editingSkiId) {
      await axios.put(`${SKIS_URL}/${editingSkiId}`, payload);
    } else {
      await axios.post(SKIS_URL, payload);
    }

    setShowAddSki(false);
    resetSkiForm();
    await loadSkis();
  } catch (err) {
    console.error(err);
    setSkiFormError(err.response?.data?.error || JSON.stringify(err.response?.data) || err.message);
  } finally {
    setSkiSaving(false);
  }
};

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setResult(null);

    try {
      const payload = {
        style: form.style,
        snowType: form.snowType,
        trackType: form.trackType,
        isSunny: form.isSunny
      };

      if (form.latitude && form.longitude) {
        payload.latitude = form.latitude;
        payload.longitude = form.longitude;
      }
      if (form.airTemp !== '') payload.airTemp = parseFloat(form.airTemp);
      if (form.humidity !== '') payload.humidity = parseFloat(form.humidity);
      if (form.windSpeed !== '') payload.windSpeed = parseFloat(form.windSpeed);

      const res = await axios.post(API_URL, payload);
      setResult(res.data);
    } catch (err) {
      console.error(err);
      setError(err.response?.data?.error || err.response?.data || err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">

<div className="tabs">
  <button
    className={`tab-btn ${activeTab === 'selection' ? 'active' : ''}`}
    onClick={() => setActiveTab('selection')}
  >
    🔍 Подбор
  </button>
  <button
    className={`tab-btn ${activeTab === 'skis' ? 'active' : ''}`}
    onClick={() => setActiveTab('skis')}
  >
    🎿 Лыжи
  </button>
  <button
    className={`tab-btn ${activeTab === 'waxes' ? 'active' : ''}`}
    onClick={() => setActiveTab('waxes')}
  >
    🧴 Мази
  </button>
</div>

      <h1>🎿 SKI-S — подбор мазей и лыж</h1>

      {activeTab === 'selection' && (
        <>
          <form onSubmit={handleSubmit} className="card">
            <h2>Условия</h2>

            <div className="form-row">
              <label>Стиль:</label>
              <select name="style" value={form.style} onChange={handleChange}>
                <option value="Classic">Классика</option>
                <option value="Free">Конёк</option>
              </select>
            </div>

            <div className="form-row">
              <label>Тип снега:</label>
              <select name="snowType" value={form.snowType} onChange={handleChange}>
                <option value="All">Любой</option>
                <option value="FreshDry">Свежий сухой</option>
                <option value="FreshWet">Свежий влажный</option>
                <option value="OldDry">Старый сухой</option>
                <option value="OldWet">Старый влажный</option>
                <option value="Transformed">Перерождённый</option>
              </select>
            </div>

            <div className="form-row">
              <label>Трасса:</label>
              <select name="trackType" value={form.trackType} onChange={handleChange}>
                <option value="All">Любая</option>
                <option value="Prepared">Подготовленная</option>
                <option value="Unprepared">Неподготовленная</option>
              </select>
            </div>

<div className="geo-block">
  <button type="button" onClick={getLocation}>📍 Моё местоположение</button>
  {form.latitude && (
    <button type="button" className="clear" onClick={clearLocation}>
      ✕ Убрать координаты
    </button>
  )}
  <p className="geo-hint">
    Нажмите — и мы определим погоду для вашего места.
  </p>
  {geoStatus && <div className="geo-status">{geoStatus}</div>}
</div>

            <p className="hint">
              Если координаты заданы — погода подтянется автоматически.
              Иначе введи вручную:
            </p>

            <div className="form-grid">
              <input
                name="airTemp"
                type="text"
                inputMode="decimal"
                placeholder="Температура °C"
                value={form.airTemp}
                onChange={handleChange}
              />
              <input
                name="humidity"
                type="text"
                inputMode="decimal"
                placeholder="Влажность %"
                value={form.humidity}
                onChange={handleChange}
              />
              <input
                name="windSpeed"
                type="text"
                inputMode="decimal"
                placeholder="Ветер м/с"
                value={form.windSpeed}
                onChange={handleChange}
              />
            </div>

            <div className="form-row checkbox">
              <label>
                <input
                  name="isSunny"
                  type="checkbox"
                  checked={form.isSunny}
                  onChange={handleChange}
                />
                {' '}Солнечно
              </label>
            </div>

            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? 'Подбираем...' : 'Подобрать'}
            </button>

            {error && <div className="error">{String(error)}</div>}
          </form>

          {result && <Results data={result} />}
        </>
      )}

      {activeTab === 'skis' && (
        <div className="card">
          <div className="arsenal-header">
            <h2>🎿 Мои лыжи ({skis.length})</h2>
            <div className="arsenal-actions">
              <button className="btn-primary-small" onClick={() => setShowAddSki(true)}>
                ➕ Добавить
              </button>
              <button className="btn-secondary" onClick={loadSkis} disabled={skisLoading}>
                {skisLoading ? '...' : '🔄'}
              </button>
            </div>
          </div>

          {skisError && <div className="error">{skisError}</div>}

          {skisLoading && skis.length === 0 && (
            <p className="hint">Загружаем список...</p>
          )}

          {!skisLoading && skis.length === 0 && !skisError && (
            <p className="hint">Список пуст. Добавьте первую пару.</p>
          )}

          {skis.map((s) => (
            <div key={s.id} className="ski-item">



<div className="ski-header">
  <span className="ski-name">{s.brand} {s.model} ({s.year})</span>
  <div className="ski-actions">
    <button className="btn-edit" onClick={() => openEditSki(s)}>✏️</button>
    <button className="btn-delete-ski" onClick={() => deleteSki(s)}>🗑️</button>
    <span className="ski-style">{s.style === 'Classic' ? 'Классика' : 'Конёк'}</span>
  </div>
</div>

              
              <div className="ski-details">
                <span>Ростовка: {s.length} см</span>
                <span>Эпюра: {s.profile}</span>
                {s.stiffnessValue && <span>Жёсткость: {s.stiffnessValue} ({s.stiffnessLabel})</span>}
                {s.camberHeightMm && <span>HBW: {s.camberHeightMm} мм</span>}
                {s.hasSkin && <span className="skin-badge">🧷 Камус</span>}
              </div>



{(s.stoneGrind || s.stoneGrindName) && (
  <div className="ski-grind">
    <b>Штайншлифт:</b> {s.stoneGrind?.name || s.stoneGrindName}
    {s.stoneGrind && ` (${s.stoneGrind.tempMin}…${s.stoneGrind.tempMax}°C, ${s.stoneGrind.trackType})`}
  </div>
)}
              
            </div>
          ))}
        </div>
      )}


{activeTab === 'waxes' && (
  <div className="card">

<div className="arsenal-header">
  <h2>🧴 Мои мази ({waxes.length})</h2>
  <div className="arsenal-actions">
    <button className="btn-primary-small" onClick={() => setShowAddWax(true)}>
      ➕ Добавить
    </button>
    <button className="btn-secondary" onClick={loadWaxes} disabled={waxesLoading}>
      {waxesLoading ? '...' : '🔄'}
    </button>
  </div>
</div>


    {waxesError && <div className="error">{waxesError}</div>}

    {waxesLoading && waxes.length === 0 && (
      <p className="hint">Загружаем список...</p>
    )}

    {!waxesLoading && waxes.length === 0 && !waxesError && (
      <p className="hint">Список мазей пуст.</p>
    )}


{waxes.map((w) => (
  <div key={w.id} className="wax-item">
    <div className="wax-header">
      <b>{w.name.startsWith(w.brand) ? w.name : `${w.brand} ${w.name}`}</b>
      <div className="ski-actions">
        <button className="btn-edit" onClick={() => openEditWax(w)}>✏️</button>
        <button className="btn-delete-ski" onClick={() => deleteWax(w)}>🗑️</button>
        <span className="wax-temp">
          {w.tempMin}…{w.tempMax}°C · {w.humidityMin}–{w.humidityMax}%
        </span>
      </div>
    </div>

<div className="ski-details">
  <span>Категория: {CATEGORY_RU[w.category] || w.category}</span>
  <span>Тип: {TYPE_RU[w.type] || w.type}</span>
  <span>Снег: {w.snowType}</span>
  <span>Трасса: {w.trackType}</span>
</div>
  
        {w.notes && <div className="wax-notes">{w.notes}</div>}
        {w.warnings && <div className="wax-warn">⚠️ {w.warnings}</div>}
      </div>
    ))}
  </div>
)}



      {/* Модальное окно — ВНЕ вкладок, внутри контейнера */}
      {showAddSki && (
        <div className="modal-overlay" onClick={() => { setShowAddSki(false); resetSkiForm(); }}>
          <div className="modal-box modal-wide" onClick={(e) => e.stopPropagation()}>
            <h3>{editingSkiId ? '✏️ Редактирование' : '➕ Новая пара лыж'}</h3>
            <div className="modal-grid">
              <div className="field">
                <label>Бренд *</label>
                <input name="brand" value={skiForm.brand} onChange={handleSkiFormChange} placeholder="Fischer" />
              </div>

              <div className="field">
                <label>Модель *</label>
                <input name="model" value={skiForm.model} onChange={handleSkiFormChange} placeholder="Speedmax 3D" />
              </div>

              <div className="field">
                <label>Год</label>
                <input name="year" type="number" value={skiForm.year} onChange={handleSkiFormChange} />
              </div>

              <div className="field">
                <label>Стиль *</label>
                <select name="style" value={skiForm.style} onChange={handleSkiFormChange}>
                  <option value="Classic">Классика</option>
                  <option value="Free">Конёк</option>
                </select>
              </div>

              <div className="field">
                <label>Ростовка (см)</label>
                <input name="length" type="number" value={skiForm.length} onChange={handleSkiFormChange} placeholder="192" />
              </div>

              <div className="field">
                <label>Эпюра</label>
                <input name="profile" value={skiForm.profile} onChange={handleSkiFormChange} placeholder="s2, cold, blue" />
              </div>

              <div className="field">
                <label>Эпюра t° min</label>
                <input name="profileTempMin" type="number" step="0.1" value={skiForm.profileTempMin} onChange={handleSkiFormChange} placeholder="-15" />
              </div>

              <div className="field">
                <label>Эпюра t° max</label>
                <input name="profileTempMax" type="number" step="0.1" value={skiForm.profileTempMax} onChange={handleSkiFormChange} placeholder="-5" />
              </div>

              <div className="field">
                <label>Жёсткость</label>
                <input name="stiffnessValue" type="number" step="0.1" value={skiForm.stiffnessValue} onChange={handleSkiFormChange} placeholder="100" />
              </div>

              <div className="field">
                <label>Единица жёсткости</label>
                <input name="stiffnessLabel" value={skiForm.stiffnessLabel} onChange={handleSkiFormChange} placeholder="FA, MF, flex" />
              </div>

              <div className="field">
                <label>HBW / hr (мм)</label>
                <input name="camberHeightMm" type="number" step="0.01" value={skiForm.camberHeightMm} onChange={handleSkiFormChange} placeholder="2.6" />
              </div>



<div className="field">
  <label>Штайншлифт</label>
  <input
    name="stoneGrindName"
    value={skiForm.stoneGrindName}
    onChange={handleSkiFormChange}
    placeholder="P5-1, SL1, X3LS..."
    list="stoneGrindSuggestions"
  />
  <datalist id="stoneGrindSuggestions">
    {stoneGrinds.map((g) => (
      <option key={g.id} value={g.name} />
    ))}
  </datalist>
</div>



              <div className="field checkbox-field">
                <label>
                  <input name="hasSkin" type="checkbox" checked={skiForm.hasSkin} onChange={handleSkiFormChange} />
                  {' '}Камус (intelligrip)
                </label>
              </div>

              <div className="field field-wide">
                <label>Заводские заметки</label>
                <textarea name="notes" value={skiForm.notes} onChange={handleSkiFormChange} rows={2} />
              </div>

              <div className="field field-wide">
                <label>Личные наблюдения</label>
                <textarea name="personalNotes" value={skiForm.personalNotes} onChange={handleSkiFormChange} rows={2} />
              </div>
            </div>

            {skiFormError && <div className="error">{skiFormError}</div>}

            <div className="modal-actions">
              <button className="cancel" onClick={() => { setShowAddSki(false); resetSkiForm(); }}>Отмена</button>
              <button className="save" onClick={saveSki} disabled={skiSaving || !skiForm.brand || !skiForm.model}>
                {skiSaving ? 'Сохраняем...' : 'Сохранить'}
              </button>
            </div>
          </div>
        </div>
      )}

      {showAddWax && (
  <div className="modal-overlay" onClick={() => { setShowAddWax(false); resetWaxForm(); }}>
    <div className="modal-box modal-wide" onClick={(e) => e.stopPropagation()}>
      <h3>{editingWaxId ? '✏️ Редактирование мази' : '➕ Новая мазь'}</h3>

      <div className="modal-grid">
        <div className="field">
          <label>Название *</label>
          <input name="name" value={waxForm.name} onChange={handleWaxFormChange} placeholder="LF Mid 0/-5" />
        </div>

        <div className="field">
          <label>Бренд *</label>
          <input name="brand" value={waxForm.brand} onChange={handleWaxFormChange} placeholder="Vauhti" />
        </div>

        <div className="field">
          <label>Категория *</label>
          <select name="category" value={waxForm.category} onChange={handleWaxFormChange}>
            <option value="Glide">Glide (скольжение)</option>
            <option value="Powder">Powder (порошок)</option>
            <option value="Grip">Grip (держание)</option>
          </select>
        </div>

        <div className="field">
          <label>Тип</label>
          <select name="type" value={waxForm.type} onChange={handleWaxFormChange}>
            <option value="Base">Base (основа)</option>
            <option value="Finish">Finish (финиш)</option>
            <option value="Universal">Universal</option>
          </select>
        </div>

        <div className="field">
          <label>Температура min (°C)</label>
          <input name="tempMin" type="number" step="0.1" value={waxForm.tempMin} onChange={handleWaxFormChange} placeholder="-25" />
        </div>

        <div className="field">
          <label>Температура max (°C)</label>
          <input name="tempMax" type="number" step="0.1" value={waxForm.tempMax} onChange={handleWaxFormChange} placeholder="-1" />
        </div>

        <div className="field">
          <label>Влажность min (%)</label>
          <input name="humidityMin" type="number" step="1" value={waxForm.humidityMin} onChange={handleWaxFormChange} placeholder="60" />
        </div>

        <div className="field">
          <label>Влажность max (%)</label>
          <input name="humidityMax" type="number" step="1" value={waxForm.humidityMax} onChange={handleWaxFormChange} placeholder="100" />
        </div>

        <div className="field">
          <label>Тип снега</label>
          <select name="snowType" value={waxForm.snowType} onChange={handleWaxFormChange}>
            <option value="All">Любой</option>
            <option value="FreshDry">Свежий сухой</option>
            <option value="FreshWet">Свежий влажный</option>
            <option value="OldDry">Старый сухой</option>
            <option value="OldWet">Старый влажный</option>
            <option value="Transformed">Перерождённый</option>
          </select>
        </div>

        <div className="field">
          <label>Тип трассы</label>
          <select name="trackType" value={waxForm.trackType} onChange={handleWaxFormChange}>
            <option value="All">Любая</option>
            <option value="Prepared">Подготовленная</option>
            <option value="Unprepared">Неподготовленная</option>
          </select>
        </div>

        <div className="field field-wide">
          <label>Заметки</label>
          <textarea name="notes" value={waxForm.notes} onChange={handleWaxFormChange} rows={2} />
        </div>

        <div className="field field-wide">
          <label>Предупреждения</label>
          <textarea name="warnings" value={waxForm.warnings} onChange={handleWaxFormChange} rows={2} />
        </div>
      </div>

      {waxFormError && <div className="error">{waxFormError}</div>}

      <div className="modal-actions">
        <button className="cancel" onClick={() => { setShowAddWax(false); resetWaxForm(); }}>Отмена</button>
        <button className="save" onClick={saveWax} disabled={waxSaving || !waxForm.name || !waxForm.brand}>
          {waxSaving ? 'Сохраняем...' : 'Сохранить'}
        </button>
      </div>
    </div>
  </div>
)}

    </div>
  );
}

function Results({ data }) {
  const input = data.input;

  return (
    <div className="results">
      <div className="card info">
        <h2>Результат подбора</h2>
        <div className="info-grid">
         
         <div>
  <b>Источник погоды:</b>{' '}
  {data.weatherSource === 'open-meteo'
    ? 'авто (Open-Meteo)'
    : 'введено вручную'}
</div>
          
          <div><b>Температура:</b> {input.airTemp}°C</div>
          <div><b>Влажность:</b> {input.humidity}%</div>
          <div><b>Эфф. влажность:</b> {input.effectiveHumidity}%</div>
          {input.windSpeed != null && <div><b>Ветер:</b> {input.windSpeed} м/с</div>}
          <div><b>Солнце:</b> {input.isSunny ? 'да' : 'нет'}</div>
          <div><b>Снег:</b> {input.snowType}</div>
          <div><b>Трасса:</b> {input.trackType}</div>
          <div><b>Стиль:</b> {input.style === 'Classic' ? 'Классика' : 'Конёк'}</div>
        </div>
        <div className="selection-id">ID подбора: <b>{data.selectionId}</b></div>
      </div>

      {data.skis?.length > 0 && (
      {(!data.skis || data.skis.length === 0) &&
 (!data.glideBase || data.glideBase.length === 0) &&
 (!data.glideFinish || data.glideFinish.length === 0) &&
 (!data.grip || data.grip.length === 0) && (
  <div className="card warning-card">
    <h2>⚠️ Ничего не найдено</h2>
    <p>
      По вашим условиям не подходит ни одна мазь и ни одна пара лыж.
      Попробуйте изменить погоду, тип снега или стиль.
    </p>
    <p className="warning-hint">
      Если считаете, что это ошибка — расскажите нам в отзыве ниже.
    </p>
  </div>
)}
        <div className="card">
          <h2>🎿 Лыжи ({data.skiCount})</h2>
          {data.skis.map((s) => (
            <div key={s.id} className="ski-item">
              <div className="ski-header">
                <span className="ski-name">{s.brand} {s.model} ({s.year})</span>
                <span className={`score ${s.matchScore >= 80 ? 'high' : s.matchScore >= 50 ? 'mid' : 'low'}`}>
                  {s.matchScore}
                </span>
              </div>
              <div className="ski-details">
                <span>Стиль: {s.style}</span>
                <span>Ростовка: {s.length} см</span>
                <span>Эпюра: {s.profile}</span>
                {s.stiffnessValue && <span>Жёсткость: {s.stiffnessValue} ({s.stiffnessLabel})</span>}
                {s.camberHeightMm && <span>HBW: {s.camberHeightMm} мм</span>}
                {s.hasSkin && <span className="skin-badge">🧷 Камус</span>}
              </div>
              {s.stoneGrind && (
                <div className="ski-grind">
                  <b>Штайншлифт:</b> {s.stoneGrind.name}
                  {' '}({s.stoneGrind.tempMin}…{s.stoneGrind.tempMax}°C, {s.stoneGrind.trackType})
                </div>
              )}
              {s.personalNotes && <div className="ski-notes">{s.personalNotes}</div>}
            </div>
          ))}
        </div>
      )}

      {data.glideBase?.length > 0 && (
        <div className="card">
          <h2>🧴 Основа скольжения ({data.glideBase.length})</h2>
          <WaxList waxes={data.glideBase} />
        </div>
      )}

      {data.glideFinish?.length > 0 && (
        <div className="card">
          <h2>✨ Финишный слой ({data.glideFinish.length})</h2>
          <WaxList waxes={data.glideFinish} />
        </div>
      )}

      {data.grip?.length > 0 && (
        <div className="card">
          <h2>🤲 Мази держания ({data.grip.length})</h2>
          <WaxList waxes={data.grip} />
        </div>
      )}

      {data.gripOptional && (
        <div className="card info-badge">
          ⚡ Все найденные лыжи с камусом — мази держания не требуются.
        </div>
      )}

      {data.notes?.length > 0 && (
        <div className="card notes-card">
          <h2>📌 Примечания</h2>
          <ul>
            {data.notes.map((n, i) => <li key={i}>{n}</li>)}
          </ul>
        </div>
      )}

      <RatingBox selectionId={data.selectionId} />
    </div>
  );
}

function WaxList({ waxes }) {
  return (
    <div className="wax-list">
      {waxes.map((w) => (
        <div key={w.id} className="wax-item">
          <div className="wax-header">
            <b>{w.name.startsWith(w.brand) ? w.name : `${w.brand} ${w.name}`}</b>
            <span className="wax-temp">
              {w.tempMin}…{w.tempMax}°C · {w.humidityMin}–{w.humidityMax}%
            </span>
          </div>
          {w.notes && <div className="wax-notes">{w.notes}</div>}
          {w.warnings && <div className="wax-warn">⚠️ {w.warnings}</div>}
        </div>
      ))}
    </div>
  );
}

function RatingBox({ selectionId }) {
  const [rating, setRating] = useState(0);
  const [review, setReview] = useState('');
  const [sent, setSent] = useState(false);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState('');

  const sendRating = async () => {
    if (rating < 1) {
      setError('Выберите оценку');
      return;
    }
    setSending(true);
    setError('');
    try {
      await axios.post(`${API_URL}/${selectionId}/rate`, {
        rating,
        review
      });
      setSent(true);
    } catch (err) {
      setError(err.response?.data?.error || err.message);
    } finally {
      setSending(false);
    }
  };

  if (sent) {
    return (
      <div className="card info-badge">
        ✅ Спасибо! Ваша оценка сохранена.
      </div>
    );
  }

  return (
    <div className="card">
      <h2>⭐ Оцените подбор</h2>
      <div className="rating-buttons">
        {[1,2,3,4,5,6,7,8,9,10].map((n) => (
          <button
            key={n}
            type="button"
            className={`rating-btn ${rating === n ? 'active' : ''}`}
            onClick={() => setRating(n)}
          >
            {n}
          </button>
        ))}
      </div>
      <textarea
        className="review-input"
        placeholder="Отзыв (необязательно): что сработало, что нет?"
        value={review}
        onChange={(e) => setReview(e.target.value)}
        rows={3}
      />
      {error && <div className="error">{error}</div>}
      <button
        className="btn-primary"
        onClick={sendRating}
        disabled={sending || rating < 1}
      >
        {sending ? 'Отправляем...' : 'Отправить оценку'}
      </button>
    </div>
  );
}

function getEmptySkiForm() {
  return {
    brand: '',
    model: '',
    year: new Date().getFullYear(),
    style: 'Classic',
    length: '',
    profile: '',
    profileTempMin: '',
    profileTempMax: '',
    stiffnessValue: '',
    stiffnessLabel: '',
    camberHeightMm: '',
    hasSkin: false,
    stoneGrindName: '',
    stoneGrindId: '',
    notes: '',
    personalNotes: ''
  };
}

function getEmptyWaxForm() {
  return {
    name: '',
    brand: '',
    category: 'Glide',
    type: 'Base',
    tempMin: '',
    tempMax: '',
    humidityMin: '',
    humidityMax: '',
    snowType: 'All',
    trackType: 'All',
    notes: '',
    warnings: ''
  };
}

export default App;
